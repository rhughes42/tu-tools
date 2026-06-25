/* Axis CNC Program CLI source. */
using System.Globalization;
using CncCli.Adapters;
using CncCli.Core;
using CncCli.MachineProfiles;
using CncCli.Observability;

namespace CncCli;

public static class Program
{
    public static async Task Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        var options = ParseArgs(args);
        var environment = options.GetValueOrDefault("environment") ?? Environment.GetEnvironmentVariable("AXIS_ENVIRONMENT") ?? "dev";
        var release = options.GetValueOrDefault("release") ?? Environment.GetEnvironmentVariable("AXIS_RELEASE") ?? "local";
        var module = "cnc-cli";

        var telemetry = new TelemetryClient(
            sampleRate: GetDouble(options, "sampleRate", GetDoubleFromEnv("AXIS_TELEMETRY_SAMPLE_RATE", 1.0)),
            new ConsoleTelemetrySink(),
            new SentryCompatibleTelemetrySink(options.GetValueOrDefault("sentryDsn") ?? Environment.GetEnvironmentVariable("AXIS_SENTRY_DSN") ?? string.Empty));

        TelemetryContext ctx = new(environment, release, module);
        await telemetry.EmitAsync("info", "CLI started", ctx);

        List<CncPoint> path = ParsePoints(options.GetValueOrDefault("points", "0,0,0;100,0,0;100,50,-5"));

        path = Calibration.Apply(
            path,
            GetDouble(options, "offsetx", 0),
            GetDouble(options, "offsety", 0),
            GetDouble(options, "offsetz", 0),
            GetDouble(options, "rotation", 0),
            GetDouble(options, "scale", 1)).ToList();

        path = Layout.Apply(
            path,
            options.GetValueOrDefault("layout", "grid"),
            (int)GetDouble(options, "rows", 1),
            (int)GetDouble(options, "cols", 1),
            GetDouble(options, "spacingx", 50),
            GetDouble(options, "spacingy", 50)).ToList();

        if (GetBool(options, "sequence", true))
        {
            path = SequencePlanner.SequenceNearestNeighbor(path).ToList();
        }

        var validationIssues = ToolpathValidator.Validate(path, GetDouble(options, "maxRapidStep", 120), GetDouble(options, "minZ", -100));

        ToolConfiguration tool = new(
            (int)GetDouble(options, "toolnumber", 1),
            GetDouble(options, "diameter", 6),
            GetDouble(options, "length", 50),
            GetDouble(options, "rpm", 12000),
            GetDouble(options, "feed", 2400),
            GetDouble(options, "plunge", 600),
            options.GetValueOrDefault("material", "Generic"),
            GetBool(options, "coolant", false));

        var physics = new PhysicsOptions(
            GetDouble(options, "cutfeed", 2400),
            GetDouble(options, "rapidfeed", 6000),
            GetDouble(options, "accel", 1500),
            GetDouble(options, "mass", 80),
            GetDouble(options, "inertia", 0.02),
            GetDouble(options, "rapidthreshold", 30),
            GetDouble(options, "spinup", 3));

        TimeEstimate estimate = TimeEstimator.Estimate(path, physics);

        List<string> commands = new();
        commands.AddRange(tool.ToHeader());
        commands.Add(CustomCommandTemplate.RenderSafe(
            options.GetValueOrDefault("template", "G1 X{X} Y{Y} Z{Z} F{Feed}"),
            options.GetValues("pair")));
        commands = ProgramTransforms.NormalizeCommands(commands).ToList();
        commands = ProgramTransforms.AddLineNumbers(commands).ToList();

        IMachineProfile profile = MachineProfileRegistry.Resolve(options.GetValueOrDefault("profile", "generic"));
        commands = profile.TransformCommands(commands).ToList();

        var profileIssues = profile.Validate(commands);
        var exportAdapter = ExportAdapterFactory.Create(options.GetValueOrDefault("exportAdapter", "plain"));
        string exported = exportAdapter.Export(path, commands);

        Console.WriteLine("=== Tool Configuration ===");
        Console.WriteLine(tool.ToSummary());

        Console.WriteLine();
        Console.WriteLine("=== Profile / Adapter ===");
        Console.WriteLine($"Profile: {profile.Name}");
        Console.WriteLine($"Export adapter: {exportAdapter.Name}");

        Console.WriteLine();
        Console.WriteLine("=== Validation ===");
        if (validationIssues.Count == 0 && profileIssues.Count == 0)
        {
            Console.WriteLine("No validation issues.");
        }
        else
        {
            foreach (var issue in validationIssues.Concat(profileIssues))
            {
                Console.WriteLine($"- {issue}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== Time Estimate (physics) ===");
        Console.WriteLine($"Distance: {estimate.Distance:F1} mm");
        Console.WriteLine($"Cutting: {estimate.CuttingSeconds:F2} s");
        Console.WriteLine($"Rapids: {estimate.RapidSeconds:F2} s");
        Console.WriteLine($"Spin-up: {estimate.SpinupSeconds:F2} s");
        Console.WriteLine($"Total: {estimate.TotalSeconds:F2} s");

        Console.WriteLine();
        Console.WriteLine("=== Export Preview ===");
        Console.WriteLine(exported);

        if (GetBool(options, "benchmark", false))
        {
            int iterations = (int)GetDouble(options, "perfIterations", 5000);
            double seconds = PerfBenchmark.Run(path, physics, iterations);
            Console.WriteLine();
            Console.WriteLine("=== Performance Baseline ===");
            Console.WriteLine($"BenchmarkIterations: {iterations}");
            Console.WriteLine($"BenchmarkSeconds: {seconds:F6}");
        }

        await telemetry.EmitAsync("info", "CLI completed", ctx, data: new Dictionary<string, string>
        {
            ["profile"] = profile.Name,
            ["adapter"] = exportAdapter.Name,
            ["points"] = path.Count.ToString(CultureInfo.InvariantCulture)
        });
    }

    private static List<CncPoint> ParsePoints(string raw)
    {
        List<CncPoint> pts = new();
        foreach (string part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] pieces = part.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (pieces.Length == 3 &&
                double.TryParse(pieces[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x) &&
                double.TryParse(pieces[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y) &&
                double.TryParse(pieces[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double z))
            {
                pts.Add(new CncPoint(x, y, z));
            }
        }

        if (pts.Count < 2)
        {
            pts.Add(new CncPoint(0, 0, 0));
            pts.Add(new CncPoint(100, 0, 0));
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
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

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

    private static bool GetBool(Dictionary<string, string> map, string key, bool fallback)
    {
        if (!map.TryGetValue(key, out string? value)) return fallback;
        if (bool.TryParse(value, out bool parsed)) return parsed;
        return fallback;
    }

    private static double GetDoubleFromEnv(string key, double fallback)
    {
        string? raw = Environment.GetEnvironmentVariable(key);
        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            return value;
        }

        return fallback;
    }
}

internal static class ArgMapExtensions
{
    public static IEnumerable<string> GetValues(this Dictionary<string, string> map, string key)
    {
        if (map.TryGetValue(key, out string? raw))
        {
            return raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        return Array.Empty<string>();
    }
}
