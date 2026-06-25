/* Axis CNC Transforms CLI source. */
namespace CncCli.Core;

internal static class Calibration
{
    public static IReadOnlyList<CncPoint> Apply(IReadOnlyList<CncPoint> pts, double ox, double oy, double oz, double rotation, double scale)
    {
        double rad = Math.PI * rotation / 180.0;
        double cos = Math.Cos(rad) * scale;
        double sin = Math.Sin(rad) * scale;

        List<CncPoint> result = new(pts.Count);
        foreach (CncPoint p in pts)
        {
            double x = p.X * cos - p.Y * sin + ox;
            double y = p.X * sin + p.Y * cos + oy;
            double z = p.Z * scale + oz;
            result.Add(new CncPoint(x, y, z));
        }
        return result;
    }
}

internal static class Layout
{
    public static IReadOnlyList<CncPoint> Apply(IReadOnlyList<CncPoint> points, string strategy, int rows, int cols, double sx, double sy)
    {
        strategy = strategy.ToLowerInvariant();
        return strategy switch
        {
            "grid" => Grid(points, Math.Max(1, rows), Math.Max(1, cols), sx, sy),
            "mirrorx" => Mirror(points, true),
            "mirrory" => Mirror(points, false),
            "rotate90" => Rotate(points, 90),
            "rotate180" => Rotate(points, 180),
            "rotate270" => Rotate(points, 270),
            _ => points
        };
    }

    private static IReadOnlyList<CncPoint> Grid(IReadOnlyList<CncPoint> points, int rows, int cols, double sx, double sy)
    {
        List<CncPoint> result = new(points.Count * rows * cols);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                double dx = c * sx;
                double dy = r * sy;
                foreach (CncPoint p in points)
                {
                    result.Add(new CncPoint(p.X + dx, p.Y + dy, p.Z));
                }
            }
        }
        return result;
    }

    private static IReadOnlyList<CncPoint> Mirror(IReadOnlyList<CncPoint> points, bool mirrorX)
    {
        List<CncPoint> result = new(points.Count * 2);
        result.AddRange(points);
        foreach (CncPoint p in points)
        {
            result.Add(mirrorX ? new CncPoint(-p.X, p.Y, p.Z) : new CncPoint(p.X, -p.Y, p.Z));
        }
        return result;
    }

    private static IReadOnlyList<CncPoint> Rotate(IReadOnlyList<CncPoint> points, double deg)
    {
        double rad = Math.PI * deg / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        List<CncPoint> result = new(points.Count);
        foreach (CncPoint p in points)
        {
            result.Add(new CncPoint(p.X * cos - p.Y * sin, p.X * sin + p.Y * cos, p.Z));
        }
        return result;
    }
}

internal static class SequencePlanner
{
    // Nearest-neighbor scheduling for traversal order.
    public static IReadOnlyList<CncPoint> SequenceNearestNeighbor(IReadOnlyList<CncPoint> points)
    {
        if (points.Count <= 2) return points;

        List<CncPoint> remaining = new(points);
        List<CncPoint> ordered = new(points.Count) { remaining[0] };
        remaining.RemoveAt(0);

        while (remaining.Count > 0)
        {
            CncPoint last = ordered[^1];
            int nearestIndex = 0;
            double nearestDistance = double.MaxValue;
            for (int i = 0; i < remaining.Count; i++)
            {
                double d = last.DistanceTo(remaining[i]);
                if (d < nearestDistance)
                {
                    nearestDistance = d;
                    nearestIndex = i;
                }
            }

            ordered.Add(remaining[nearestIndex]);
            remaining.RemoveAt(nearestIndex);
        }

        return ordered;
    }
}
