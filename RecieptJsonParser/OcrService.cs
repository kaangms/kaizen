using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RecieptJsonParser.Models;

namespace RecieptJsonParser;

public static class OcrService
{
    public static void WriteToText()
    {
        var ocrTextBlockWrapper = ReadFileData();
        var datas = CreateLineList(ocrTextBlockWrapper);
        // The file path may vary depending on your system
        using (var writer = new StreamWriter("../../../output.txt"))
        {
            foreach (var line in datas)
            {
                var lineText = string.Join(" ", line.Value.Select(block => block.Description));
                writer.WriteLine(lineText);
            }
        }
    }
    public static OcrTextBlockWrapper ReadFileData()
    {
        List<OcrTextBlock> ocrTextBlocks;
        // The file path may vary depending on your system
        using (var reader = new StreamReader("../../../response.json"))
        {
            string jsonContent = reader.ReadToEnd();
            ocrTextBlocks = JsonConvert.DeserializeObject<List<OcrTextBlock>>(jsonContent) ?? [];
        }
        // Stores the slope (angle) between adjacent OCR boxes,
        // used to estimate the average text line angle (baseline skew).
        List<double> slopes = [];

        // Stores the height of each OCR bounding box,
        // used to calculate the average character or line height for filtering purposes.
        List<double> boundingBoxHighs = [];

        for (int i = 0; i < ocrTextBlocks.Count; i++)
        {
            if (i == 0) continue;

            var vertices = ocrTextBlocks[i].BoundingPoly?.Vertices;

            if (vertices == null || vertices.Count != 4) throw new Exception("Vertices are null or less than 4");

            vertices = [.. vertices
            .OrderBy(v => v.Y)
            .Chunk(2)
            .Select(group => group.OrderBy(v => v.X))
            .SelectMany(group => group)];

            //Calculate the slope of the top and bottom lines
            var topSlope = AnalyzerHelper.CalculateSlope(vertices[0], vertices[1]);
            slopes.Add(topSlope);

            var bottomSlope = AnalyzerHelper.CalculateSlope(vertices[2], vertices[3]);
            slopes.Add(bottomSlope);

            //Calculate the height of the bounding box
            var leftheight = Math.Abs(vertices[0].Y - vertices[2].Y);
            boundingBoxHighs.Add(leftheight);
            var rightheight = Math.Abs(vertices[1].Y - vertices[3].Y);
            boundingBoxHighs.Add(rightheight);
        }

        var occrTexxtBlockWrapper = new OcrTextBlockWrapper
        {
            HighAverage = AnalyzerHelper.GetReliableSkew(boundingBoxHighs),
            SlopeAverage = AnalyzerHelper.GetReliableSkew(slopes),
            OcrTextBlockMain = ocrTextBlocks[0],
            OcrTextBlocks = [.. ocrTextBlocks.Skip(1)],
        };

        foreach (var item in occrTexxtBlockWrapper.OcrTextBlocks)
        {
            var height = Math.Max(GetLeftHeightForBox(item.BoundingPoly!.Vertices), GetRightHeightForBox(item.BoundingPoly!.Vertices));

            bool isOutOfNormalHeightRange = height >= occrTexxtBlockWrapper.BigBlobBoxHigh || height <= occrTexxtBlockWrapper.NoiseBlobBox;

            if (isOutOfNormalHeightRange)
            {
                occrTexxtBlockWrapper.BlobOcrTextBlocks.Add(item);
            }
            else
            {
                occrTexxtBlockWrapper.FiltredOcrTextBlocks.Add(item);
            }

        }
        return occrTexxtBlockWrapper;
    }
    private static Dictionary<int, List<OcrTextBlock>> CreateLineList(OcrTextBlockWrapper ocrDataWrapper)
    {
        var lineIndex = 0;
        var lineOcrTextBlockDictionary = new Dictionary<int, List<OcrTextBlock>>();
        while (ocrDataWrapper.FiltredOcrTextBlocks.Count > 0)
        {
            var topLeftOcrBox = ocrDataWrapper.FiltredOcrTextBlocks.
               OrderBy(x => x.BoundingPoly!.Vertices[0].Y)
              .ThenBy(x => x.BoundingPoly!.Vertices[0].X)
              .FirstOrDefault();

            var minX = topLeftOcrBox!.BoundingPoly!.Vertices[0].X;
            var maxX = topLeftOcrBox!.BoundingPoly!.Vertices[1].X;

            var filteredVertices = new List<Vertex>
                                    {
                                        new () { X = minX, Y = 0 }, // index 0
                                        new () { X = maxX, Y = 0 }, // index 1
                                        new () { X = minX, Y = 0 }, // index 2
                                        new () { X = maxX, Y = 0 }, // index 3
                                    };
            //set index 0 
            var width_Dist_OcrCapacityBox_To_TopLeftOcrBox = GetDistance(minX, topLeftOcrBox!.BoundingPoly!.Vertices[0].X);
            filteredVertices[0].Y = ComputeYDotRL(topLeftOcrBox!.BoundingPoly!.Vertices[0].Y, width_Dist_OcrCapacityBox_To_TopLeftOcrBox, ocrDataWrapper.SlopeAverage);
            //set index 2
            filteredVertices[2].Y = filteredVertices[0].Y + GetLeftHeightForBox(topLeftOcrBox.BoundingPoly!.Vertices);
            //set index 1
            filteredVertices[1].Y = ComputeYDotLR(filteredVertices[0].Y, GetDistance(minX, maxX), ocrDataWrapper.SlopeAverage);

            var leftBaseLineDotY = (filteredVertices[0].Y + filteredVertices[2].Y) / 2;

            List<OcrTextBlock> lineItems = [];
            int maxDistanceHightBasLine = 0;
            for (int i = 0; i < ocrDataWrapper.FiltredOcrTextBlocks.Count; i++)
            {
                var currentBaseLineDotY = ComputeYDotLR(leftBaseLineDotY, GetDistance(minX, ocrDataWrapper.FiltredOcrTextBlocks[i].BoundingPoly!.Vertices[2].X), ocrDataWrapper.SlopeAverage);
                if (ocrDataWrapper.FiltredOcrTextBlocks[i].BoundingPoly!.Vertices[2].Y > currentBaseLineDotY && ocrDataWrapper.FiltredOcrTextBlocks[i].BoundingPoly!.Vertices[0].Y < currentBaseLineDotY)
                {
                    lineItems.Add(ocrDataWrapper.FiltredOcrTextBlocks[i]);
                    var distanceHightBasLine = ocrDataWrapper.FiltredOcrTextBlocks[i].BoundingPoly!.Vertices[2].Y - currentBaseLineDotY;
                    if (ocrDataWrapper.FiltredOcrTextBlocks[i].BoundingPoly!.Vertices[0].Y < filteredVertices[0].Y)
                    {
                        filteredVertices[0].Y = ocrDataWrapper.FiltredOcrTextBlocks[i].BoundingPoly!.Vertices[0].Y;

                        if (distanceHightBasLine > maxDistanceHightBasLine)
                        {
                            maxDistanceHightBasLine = distanceHightBasLine;
                            filteredVertices = ocrDataWrapper.FiltredOcrTextBlocks[i].BoundingPoly!.Vertices;
                        }
                    }
                }
            }
            ocrDataWrapper.FiltredOcrTextBlocks.RemoveAll(lineItems.Contains);
            lineOcrTextBlockDictionary.Add(lineIndex, lineItems);
            var leftBaseLineDotYBottom = ComputeYDotRL(leftBaseLineDotY, GetDistance(minX, filteredVertices[2].X), ocrDataWrapper.SlopeAverage);

            if (ocrDataWrapper.BlobOcrTextBlocks.Count > 0)
            {

                lineItems = [.. ocrDataWrapper.BlobOcrTextBlocks.Where(x =>
            ComputeYDotLR(leftBaseLineDotYBottom, GetDistance(minX, x.BoundingPoly!.Vertices[0].X), ocrDataWrapper.SlopeAverage)>
            x.BoundingPoly!.Vertices[0].Y )];
                ocrDataWrapper.BlobOcrTextBlocks.RemoveAll(lineItems.Contains);
                lineOcrTextBlockDictionary.Add(lineIndex, lineItems);
            }
            lineIndex++;
            if (ocrDataWrapper.FiltredOcrTextBlocks.Count == 0 && ocrDataWrapper.BlobOcrTextBlocks.Count > 0)
            {
                lineOcrTextBlockDictionary.Add(lineIndex, ocrDataWrapper.BlobOcrTextBlocks);
            }
        }
        return lineOcrTextBlockDictionary;
    }
    static int GetLeftHeightForBox(List<Vertex> vertices) => Math.Abs(vertices[0].Y - vertices[2].Y);
    static int GetRightHeightForBox(List<Vertex> vertices) => Math.Abs(vertices[1].Y - vertices[3].Y);
    static int GetDistance(int k0, int k1) => Math.Abs(k0 - k1);
    //Calculate Generic Y Dot Value From Left To Right
    static int ComputeYDotLR(int yLeft, int width, double slopeAverage) => yLeft - (int)Math.Round(width * slopeAverage);
    static int ComputeYDotRL(int yRight, int width, double slopeAverage) => yRight + (int)Math.Round(width * slopeAverage);

}
