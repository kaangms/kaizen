using System;
using System.Collections.Generic;
using System.Linq;
using RecieptJsonParser.Models;
namespace RecieptJsonParser;

public static class AnalyzerHelper
{
    public static double GetReliableSkew(List<double> slopes)
    {
        if (slopes == null || slopes.Count == 0)
            throw new ArgumentException("Angle list is empty");

        // Step 1: Trim outliers
        var trimmed = IQRFilter(slopes);
        // Step 2: Comparing median and mean
        double median = Median(trimmed);
        return median;
    }

    public static List<double> IQRFilter(List<double> slopes)
    {
        var sorted = slopes.OrderBy(x => x).ToList();
        int n = sorted.Count;
        double q1 = sorted[(int)(n * 0.25)];
        double q3 = sorted[(int)(n * 0.75)];
        double iqr = q3 - q1;
        double lowerBound = q1 - 1.5 * iqr;
        double upperBound = q3 + 1.5 * iqr;
        return [.. sorted.Where(x => x >= lowerBound && x <= upperBound)];
    }

    private static double Median(List<double> data)
    {
        var sorted = data.OrderBy(x => x).ToList();
        int mid = sorted.Count / 2;
        return (sorted.Count % 2 == 0)
            ? (sorted[mid - 1] + sorted[mid]) / 2.0
            : sorted[mid];
    }
    public static double CalculateSlope(Vertex leftVertex, Vertex rightVertex)
    {
        double deltaY = leftVertex.Y - rightVertex.Y;
        double deltaX = rightVertex.X - leftVertex.X;
        return deltaY / deltaX;
    }

}
