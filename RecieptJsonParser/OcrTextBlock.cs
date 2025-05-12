using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecieptJsonParser.Models;
public class OcrTextBlockWrapper
{
    public required OcrTextBlock OcrTextBlockMain { get; set; } 
    public List<OcrTextBlock> OcrTextBlocks { get; set; } = [];
    public List<OcrTextBlock> BlobOcrTextBlocks { get; set; } = [];
    public List<OcrTextBlock> FiltredOcrTextBlocks { get; set; } = [];
    public double SlopeAverage { get; set; } = 0;
    public double HighAverage { get; set; } = 0;
    public double BigBlobBoxHigh=> HighAverage * Constants.kMaxTableCellXheight;
    public double NoiseBlobBox => HighAverage * Constants.kAllowBlobHeight;


}
public class OcrTextBlock
{
    public string? Locale { get; set; } 
    public string? Description { get; set; } 
    public BoundingPoly? BoundingPoly { get; set; } 
}

public class BoundingPoly
{
    public List<Vertex> Vertices { get; set; } = [];
}

public class Vertex
{
    public int X { get; set; }
    public int Y { get; set; }
}
