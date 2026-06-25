/* Axis CNC ValidationAndTime CLI source. */
namespace CncCli.Core;

internal readonly record struct PhysicsOptions(
    double CutFeed,
    double RapidFeed,
    double Acceleration,
    double Mass,
    double InertiaFactor,
    double RapidThreshold,
    double SpinupSeconds);

internal readonly record struct TimeEstimate(double CuttingSeconds, double RapidSeconds, double SpinupSeconds, double Distance)
{
    public double TotalSeconds => CuttingSeconds + RapidSeconds + SpinupSeconds;
}

internal static class ToolpathValidator
{
    public static IReadOnlyList<string> Validate(IReadOnlyList<CncPoint> points, double maxRapidStepMm, double minAllowedZ)
    {
        List<string> issues = new();
        if (points.Count < 2)
        {
            issues.Add("Toolpath must contain at least two points.");
            return issues;
        }

        for (int i = 1; i < points.Count; i++)
        {
            var prev = points[i - 1];
            var curr = points[i];
            double step = prev.DistanceTo(curr);

            if (step > maxRapidStepMm)
            {
                issues.Add($"Large move detected at segment {i}: {step:F2} mm.");
            }

            if (curr.Z < minAllowedZ)
            {
                issues.Add($"Z below safe limit at point {i}: {curr.Z:F2}.");
            }
        }

        return issues;
    }
}

internal static class TimeEstimator
{
    public static TimeEstimate Estimate(IReadOnlyList<CncPoint> path, PhysicsOptions options)
    {
        if (path.Count < 2) return new TimeEstimate(0, 0, Math.Max(0, options.SpinupSeconds), 0);

        double cutting = 0;
        double rapids = 0;
        double distance = 0;
        double effectiveAccel = Math.Max(1e-6, options.Acceleration / Math.Max(0.5, 1.0 + options.Mass * options.InertiaFactor));

        for (int i = 1; i < path.Count; i++)
        {
            CncPoint a = path[i - 1];
            CncPoint b = path[i];
            double d = a.DistanceTo(b);
            distance += d;

            bool isRapid = d >= options.RapidThreshold || a.Z < b.Z;
            double feed = Math.Max(1e-6, (isRapid ? options.RapidFeed : options.CutFeed) / 60.0);

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

            if (isRapid) rapids += segment;
            else cutting += segment;
        }

        return new TimeEstimate(cutting, rapids, Math.Max(0, options.SpinupSeconds), distance);
    }
}

internal static class PerfBenchmark
{
    public static double Run(IReadOnlyList<CncPoint> path, PhysicsOptions physics, int iterations)
    {
        iterations = Math.Max(1, iterations);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        double total = 0;
        for (int i = 0; i < iterations; i++)
        {
            total += TimeEstimator.Estimate(path, physics).TotalSeconds;
        }
        sw.Stop();
        _ = total;
        return sw.Elapsed.TotalSeconds;
    }
}
