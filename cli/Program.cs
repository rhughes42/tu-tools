using System.Globalization;

namespace CncCli;

internal record Point(double X, double Y, double Z)
{
    public double DistanceTo(Point other)
    {
        double dx = X - other.X;
        double dy = Y - other.Y;
        double dz = Z - other.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}

internal record ToolConfiguration(
    int ToolNumber,
    double Diameter,
    double Length,
    double SpindleRpm,
    double FeedRate,
    double PlungeRate,
    string Material,
    bool CoolantOn)
{
    public IEnumerable<string> ToHeader()
    {
        yield return $"( Tool {ToolNumber} | Dia {Diameter:F2}mm | {Material} )";
        yield return $"T{ToolNumber} M06";
        yield return $"S{SpindleRpm:F0} M03";
        yield return $"F{FeedRate:F1}";
        if (CoolantOn) yield return "M08";
        yield return $"( Plunge {PlungeRate:F1} mm/min )";
    }

    public string ToSummary() =>
        $"T{ToolNumber} Ø{Diameter:F1} L{Length:F0} | {SpindleRpm:F0} rpm | F{FeedRate:F0} / P{PlungeRate:F0} | {(CoolantOn ? "Coolant" : "Dry")} ({Material})";
}

internal record PhysicsOptions(
    double CutFeed,
    double RapidFeed,
    double Acceleration,
    double Mass,
    double InertiaFactor,
    double RapidThreshold,
    double SpinupSeconds);

internal record TimeEstimate(double CuttingSeconds, double RapidSeconds, double SpinupSeconds, double Distance)
{
    public double TotalSeconds => CuttingSeconds + RapidSeconds + SpinupSeconds;
}

internal static class TimeEstimator
{
    public static TimeEstimate Estimate(IReadOnlyList<Point> path, PhysicsOptions options)
    {
        if (path.Count < 2) return new TimeEstimate(0, 0, options.SpinupSeconds, 0);

        double cutting = 0;
        double rapids = 0;
        double distance = 0;
        double effectiveAccel = options.Acceleration / Math.Max(0.5, 1.0 + options.Mass * options.InertiaFactor);

        for (int i = 1; i < path.Count; i++)
        {
            Point a = path[i - 1];
            Point b = path[i];
            double d = a.DistanceTo(b);
            distance += d;

            bool isRapid = d >= options.RapidThreshold || a.Z < b.Z;
            double feed = (isRapid ? options.RapidFeed : options.CutFeed) / 60.0;

            double accelTime = feed / effectiveAccel;
            double accelDist = 0.5 * effectiveAccel * accelTime * accelTime;
            double segment;
            if (2 * accelDist >= d)
            {
                segment = 2 * Math.Sqrt(d / effectiveAccel);
            }
            else
            {
                double cruise = d - 2 * accelDist;
                segment = 2 * accelTime + cruise / feed;
            }

            if (isRapid) rapids += segment; else cutting += segment;
        }

        return new TimeEstimate(cutting, rapids, Math.Max(0, options.SpinupSeconds), distance);
    }
}

internal static class Layout
{
    public static IReadOnlyList<Point> Apply(IReadOnlyList<Point> points, string strategy, int rows, int cols, double sx, double sy)
    {
        strategy = strategy.ToLowerInvariant();
        return strategy switch
        {
            "grid" => Grid(points, rows, cols, sx, sy),
            "mirrorx" => Mirror(points, true),
            "mirrory" => Mirror(points, false),
            "rotate90" => Rotate(points, 90),
            "rotate180" => Rotate(points, 180),
            "rotate270" => Rotate(points, 270),
            _ => points
        };
    }

    private static IReadOnlyList<Point> Grid(IReadOnlyList<Point> points, int rows, int cols, double sx, double sy)
    {
        List<Point> result = new(points.Count * rows * cols);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                double dx = c * sx;
                double dy = r * sy;
                foreach (Point p in points)
                {
                    result.Add(new Point(p.X + dx, p.Y + dy, p.Z));
                }
            }
        }
        return result;
    }

    private static IReadOnlyList<Point> Mirror(IReadOnlyList<Point> points, bool mirrorX)
    {
        List<Point> result = new(points.Count * 2);
        result.AddRange(points);
        foreach (Point p in points)
        {
            result.Add(mirrorX ? new Point(-p.X, p.Y, p.Z) : new Point(p.X, -p.Y, p.Z));
        }
        return result;
    }

    private static IReadOnlyList<Point> Rotate(IReadOnlyList<Point> points, double deg)
    {
        double rad = Math.PI * deg / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        List<Point> result = new(points.Count);
        foreach (Point p in points)
        {
            double x = p.X * cos - p.Y * sin;
            double y = p.X * sin + p.Y * cos;
            result.Add(new Point(x, y, p.Z));
        }
        return result;
    }
}

internal static class Calibration
{
    public static IReadOnlyList<Point> Apply(IReadOnlyList<Point> pts, double ox, double oy, double oz, double rotation, double scale)
    {
        double rad = Math.PI * rotation / 180.0;
        double cos = Math.Cos(rad) * scale;
        double sin = Math.Sin(rad) * scale;

        List<Point> result = new(pts.Count);
        foreach (Point p in pts)
        {
            double x = p.X * cos - p.Y * sin + ox;
            double y = p.X * sin + p.Y * cos + oy;
            double z = p.Z * scale + oz;
            result.Add(new Point(x, y, z));
        }
        return result;
    }
}

internal static class CustomCommand
{
    public static string Render(string template, IEnumerable<string> pairs)
    {
        Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);
        foreach (string pair in pairs)
        {
            var split = pair.Split('=', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 2)
            {
                values[split[0]] = split[1];
            }
        }

        string result = template;
        foreach (var kvp in values)
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
        }

        return result;
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        var options = ParseArgs(args);

        List<Point> basePath = ParsePoints(options.GetValueOrDefault("points", "0,0,0;100,0,0;100,50,-5"));

        basePath = Calibration.Apply(
            basePath,
            GetDouble(options, "offsetx", 0),
            GetDouble(options, "offsety", 0),
            GetDouble(options, "offsetz", 0),
            GetDouble(options, "rotation", 0),
            GetDouble(options, "scale", 1)).ToList();

        basePath = Layout.Apply(
            basePath,
            options.GetValueOrDefault("layout", "grid"),
            (int)GetDouble(options, "rows", 1),
            (int)GetDouble(options, "cols", 1),
            GetDouble(options, "spacingx", 50),
            GetDouble(options, "spacingy", 50)).ToList();

        ToolConfiguration tool = new(
            (int)GetDouble(options, "toolnumber", 1),
            GetDouble(options, "diameter", 6),
            GetDouble(options, "length", 50),
            GetDouble(options, "rpm", 12000),
            GetDouble(options, "feed", 2400),
            GetDouble(options, "plunge", 600),
            options.GetValueOrDefault("material", "Generic"),
            options.ContainsKey("coolant") && options["coolant"].Equals("true", StringComparison.OrdinalIgnoreCase));

        PhysicsOptions physics = new(
            GetDouble(options, "cutfeed", 2400),
            GetDouble(options, "rapidfeed", 6000),
            GetDouble(options, "accel", 1500),
            GetDouble(options, "mass", 80),
            GetDouble(options, "inertia", 0.02),
            GetDouble(options, "rapidthreshold", 30),
            GetDouble(options, "spinup", 3));

        TimeEstimate estimate = TimeEstimator.Estimate(basePath, physics);

        Console.WriteLine("=== Tool Configuration ===");
        Console.WriteLine(tool.ToSummary());
        foreach (string line in tool.ToHeader())
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        Console.WriteLine("=== Custom Command ===");
        string template = options.GetValueOrDefault("template", "G1 X{X} Y{Y} Z{Z} F{Feed}");
        string command = CustomCommand.Render(template, options.GetValues("pair"));
        Console.WriteLine(string.IsNullOrWhiteSpace(command) ? "(no command)" : command);

        Console.WriteLine();
        Console.WriteLine("=== Time Estimate (physics) ===");
        Console.WriteLine($"Distance: {estimate.Distance:F1} mm");
        Console.WriteLine($"Cutting: {estimate.CuttingSeconds:F2} s");
        Console.WriteLine($"Rapids: {estimate.RapidSeconds:F2} s");
        Console.WriteLine($"Spin-up: {estimate.SpinupSeconds:F2} s");
        Console.WriteLine($"Total: {estimate.TotalSeconds:F2} s");
    }

    private static List<Point> ParsePoints(string raw)
    {
        List<Point> pts = new();
        foreach (string part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] pieces = part.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (pieces.Length == 3 &&
                double.TryParse(pieces[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x) &&
                double.TryParse(pieces[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y) &&
                double.TryParse(pieces[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double z))
            {
                pts.Add(new Point(x, y, z));
            }
        }
        if (pts.Count < 2)
        {
            pts.Add(new Point(0, 0, 0));
            pts.Add(new Point(100, 0, 0));
        }
        return pts;
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        Dictionary<string, string> map = new(StringComparer.OrdinalIgnoreCase);
        List<string> pairs = new();
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg.StartsWith("--"))
            {
                string key = arg[2..];
                if (key.Equals("pair", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    pairs.Add(args[++i]);
                    continue;
                }

                if (i + 1 < args.Length)
                {
                    map[key] = args[++i];
                }
            }
        }

        if (pairs.Count > 0)
        {
            map["pair"] = string.Join('|', pairs);
        }

        return map;
    }

    private static double GetDouble(Dictionary<string, string> map, string key, double fallback)
    {
        if (map.TryGetValue(key, out string? value) &&
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
        {
            return parsed;
        }
        return fallback;
    }

    private static IEnumerable<string> GetValues(this Dictionary<string, string> map, string key)
    {
        if (map.TryGetValue(key, out string? raw))
        {
            return raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        return Array.Empty<string>();
    }
}
