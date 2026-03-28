using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rhino.Geometry;

namespace TUTools.CNC
{
    /// <summary>
    /// Supported CNC component and operation types.
    /// </summary>
    public enum ComponentOperationType
    {
        LinearCut,
        ArcCut,
        Drill,
        Pocket,
        Engrave,
        Probe,
        Facing,
        Custom
    }

    /// <summary>
    /// Supported layout strategies for arranging toolpaths or workpieces.
    /// </summary>
    public enum LayoutStrategy
    {
        None,
        Grid,
        MirrorX,
        MirrorY,
        Rotate90,
        Rotate180,
        Rotate270
    }

    /// <summary>
    /// Represents calibration offsets and transforms that can be applied to toolpaths.
    /// </summary>
    public class CalibrationOffset
    {
        public CalibrationOffset(double x, double y, double z, double rotationDegrees, double scale)
        {
            X = x;
            Y = y;
            Z = z;
            RotationDegrees = rotationDegrees;
            Scale = scale;
        }

        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public double RotationDegrees { get; }
        public double Scale { get; }

        /// <summary>
        /// Applies the stored transform to a collection of points.
        /// </summary>
        public IList<Point3d> Apply(IEnumerable<Point3d> points)
        {
            if (points == null) return new List<Point3d>();

            Transform xform = Transform.Scale(Point3d.Origin, Scale);
            xform = xform * Transform.Rotation(RotationDegrees * Math.PI / 180.0, Point3d.Origin);
            xform = xform * Transform.Translation(X, Y, Z);

            List<Point3d> result = new List<Point3d>();
            foreach (Point3d p in points)
            {
                Point3d transformed = p;
                transformed.Transform(xform);
                result.Add(transformed);
            }

            return result;
        }
    }

    /// <summary>
    /// Describes layout options for replicating toolpaths on the workspace.
    /// </summary>
    public class LayoutOptions
    {
        public LayoutOptions(LayoutStrategy strategy, int rows, int columns, double spacingX, double spacingY)
        {
            Strategy = strategy;
            Rows = Math.Max(1, rows);
            Columns = Math.Max(1, columns);
            SpacingX = spacingX;
            SpacingY = spacingY;
        }

        public LayoutStrategy Strategy { get; }
        public int Rows { get; }
        public int Columns { get; }
        public double SpacingX { get; }
        public double SpacingY { get; }

        /// <summary>
        /// Generates copies of a toolpath according to the layout strategy.
        /// </summary>
        public IList<Point3d> Apply(IEnumerable<Point3d> path)
        {
            if (path == null) return new List<Point3d>();

            List<Point3d> source = new List<Point3d>(path);
            if (source.Count == 0) return source;

            switch (Strategy)
            {
                case LayoutStrategy.Grid:
                    return ApplyGrid(source);
                case LayoutStrategy.MirrorX:
                    return ApplyMirror(source, true);
                case LayoutStrategy.MirrorY:
                    return ApplyMirror(source, false);
                case LayoutStrategy.Rotate90:
                    return ApplyRotation(source, 90.0);
                case LayoutStrategy.Rotate180:
                    return ApplyRotation(source, 180.0);
                case LayoutStrategy.Rotate270:
                    return ApplyRotation(source, 270.0);
                default:
                    return source;
            }
        }

        private IList<Point3d> ApplyGrid(IReadOnlyList<Point3d> path)
        {
            List<Point3d> result = new List<Point3d>(path.Count * Rows * Columns);
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    double offsetX = c * SpacingX;
                    double offsetY = r * SpacingY;
                    foreach (Point3d p in path)
                    {
                        result.Add(new Point3d(p.X + offsetX, p.Y + offsetY, p.Z));
                    }
                }
            }
            return result;
        }

        private static IList<Point3d> ApplyMirror(IReadOnlyList<Point3d> path, bool mirrorX)
        {
            List<Point3d> mirrored = new List<Point3d>(path.Count * 2);
            mirrored.AddRange(path);

            foreach (Point3d p in path)
            {
                mirrored.Add(mirrorX ? new Point3d(-p.X, p.Y, p.Z) : new Point3d(p.X, -p.Y, p.Z));
            }

            return mirrored;
        }

        private static IList<Point3d> ApplyRotation(IReadOnlyList<Point3d> path, double degrees)
        {
            Transform rotation = Transform.Rotation(degrees * Math.PI / 180.0, Point3d.Origin);
            List<Point3d> rotated = new List<Point3d>(path.Count);
            foreach (Point3d p in path)
            {
                Point3d transformed = p;
                transformed.Transform(rotation);
                rotated.Add(transformed);
            }
            return rotated;
        }
    }

    /// <summary>
    /// Captures the tool configuration used for a CNC operation.
    /// </summary>
    public class ToolConfiguration
    {
        public ToolConfiguration(int toolNumber, double diameter, double length, double spindleRpm, double feedRate, double plungeRate, string material, bool coolantOn)
        {
            ToolNumber = toolNumber;
            Diameter = diameter;
            Length = length;
            SpindleRpm = spindleRpm;
            FeedRate = feedRate;
            PlungeRate = plungeRate;
            Material = material ?? "Generic";
            CoolantOn = coolantOn;
        }

        public int ToolNumber { get; }
        public double Diameter { get; }
        public double Length { get; }
        public double SpindleRpm { get; }
        public double FeedRate { get; }
        public double PlungeRate { get; }
        public string Material { get; }
        public bool CoolantOn { get; }

        /// <summary>
        /// Generates a concise summary string for UI display.
        /// </summary>
        public string ToSummary()
        {
            return $"T{ToolNumber} | Ø{Diameter:F2}mm L{Length:F1}mm | {SpindleRpm:F0} rpm | F{FeedRate:F1} / P{PlungeRate:F1} mm/min | {(CoolantOn ? "Coolant On" : "Dry")} ({Material})";
        }

        /// <summary>
        /// Builds machine-ready command lines for the configuration.
        /// </summary>
        public IList<string> ToGCodeHeader()
        {
            List<string> lines = new List<string>
            {
                $"( Tool {ToolNumber} | Dia {Diameter:F2}mm | {Material} )",
                $"T{ToolNumber}M06",
                $"G43H{ToolNumber}",
                $"S{SpindleRpm:F0}M03",
                $"F{FeedRate:F1}"
            };

            if (CoolantOn)
            {
                lines.Add("M08");
            }

            lines.Add($"( Plunge {PlungeRate:F1} mm/min )");
            return lines;
        }
    }

    /// <summary>
    /// Defines a custom command template with token replacement.
    /// </summary>
    public class CustomCommandDefinition
    {
        public CustomCommandDefinition(string name, string template, IDictionary<string, string> defaults = null)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Custom" : name.Trim();
            Template = string.IsNullOrWhiteSpace(template) ? string.Empty : template;
            Defaults = defaults ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string Name { get; }
        public string Template { get; }
        public IDictionary<string, string> Defaults { get; }

        /// <summary>
        /// Renders the command using supplied overrides (merged with defaults).
        /// </summary>
        public string Render(IDictionary<string, string> overrides = null)
        {
            if (string.IsNullOrWhiteSpace(Template))
            {
                return string.Empty;
            }

            Dictionary<string, string> merged = new Dictionary<string, string>(Defaults, StringComparer.OrdinalIgnoreCase);
            if (overrides != null)
            {
                foreach (var kvp in overrides)
                {
                    merged[kvp.Key] = kvp.Value;
                }
            }

            string output = Template;
            foreach (var kvp in merged)
            {
                output = output.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            return output;
        }
    }

    /// <summary>
    /// Options for estimating real-world cutting time.
    /// </summary>
    public class CutPhysicsOptions
    {
        public CutPhysicsOptions(double cutFeed, double rapidFeed, double acceleration, double massKg, double inertiaFactor, double rapidThreshold)
        {
            CutFeed = cutFeed;
            RapidFeed = rapidFeed;
            Acceleration = acceleration;
            MassKg = massKg;
            InertiaFactor = inertiaFactor;
            RapidThreshold = rapidThreshold;
        }

        /// <summary>Feed rate during cutting (mm/min).</summary>
        public double CutFeed { get; }

        /// <summary>Feed rate during rapid moves (mm/min).</summary>
        public double RapidFeed { get; }

        /// <summary>Maximum acceleration (mm/s^2).</summary>
        public double Acceleration { get; }

        /// <summary>Moving mass (kg).</summary>
        public double MassKg { get; }

        /// <summary>Multiplier to account for inertia of gantry/tooling.</summary>
        public double InertiaFactor { get; }

        /// <summary>Distance threshold that classifies a move as rapid.</summary>
        public double RapidThreshold { get; }

        /// <summary>
        /// Gets an effective acceleration that is reduced by mass and inertia.
        /// </summary>
        public double EffectiveAcceleration => Acceleration / Math.Max(0.5, 1.0 + MassKg * InertiaFactor);
    }

    /// <summary>
    /// Result of the physics-based cut time estimation.
    /// </summary>
    public class CutTimeEstimate
    {
        public double CuttingSeconds { get; set; }
        public double RapidSeconds { get; set; }
        public double SpindleSeconds { get; set; }
        public double TotalDistanceMm { get; set; }
        public double TotalSeconds => CuttingSeconds + RapidSeconds + SpindleSeconds;

        public IList<string> ToReport()
        {
            return new List<string>
            {
                $"Total distance: {TotalDistanceMm:F2} mm",
                $"Cutting: {Format(CuttingSeconds)}",
                $"Rapids: {Format(RapidSeconds)}",
                $"Spindle/overhead: {Format(SpindleSeconds)}",
                $"Total: {Format(TotalSeconds)}"
            };
        }

        private static string Format(double seconds)
        {
            if (seconds < 60) return seconds.ToString("F1", CultureInfo.InvariantCulture) + "s";
            if (seconds < 3600) return $"{(int)(seconds / 60)}m {(int)(seconds % 60)}s";
            return $"{(int)(seconds / 3600)}h {(int)((seconds % 3600) / 60)}m";
        }
    }

    /// <summary>
    /// Provides physics-aware cut time estimation using a trapezoidal velocity profile.
    /// </summary>
    public static class CutTimeEstimator
    {
        public static CutTimeEstimate EstimatePath(IReadOnlyList<Point3d> path, CutPhysicsOptions options, double spindleSpinUpSeconds = 3.0)
        {
            if (path == null || path.Count < 2)
            {
                return new CutTimeEstimate();
            }

            double cuttingSeconds = 0.0;
            double rapidSeconds = 0.0;
            double totalDistance = 0.0;
            double accel = Math.Max(1.0, options.EffectiveAcceleration);

            for (int i = 1; i < path.Count; i++)
            {
                Point3d a = path[i - 1];
                Point3d b = path[i];
                double distance = a.DistanceTo(b);
                totalDistance += distance;

                bool isRapid = distance >= options.RapidThreshold || a.Z > b.Z;
                double feed = isRapid ? options.RapidFeed : options.CutFeed;
                double feedPerSecond = Math.Max(1.0, feed / 60.0);

                double segmentTime = ComputeTrapezoidalTime(distance, feedPerSecond, accel);

                if (isRapid)
                {
                    rapidSeconds += segmentTime;
                }
                else
                {
                    cuttingSeconds += segmentTime;
                }
            }

            return new CutTimeEstimate
            {
                CuttingSeconds = cuttingSeconds,
                RapidSeconds = rapidSeconds,
                SpindleSeconds = Math.Max(0, spindleSpinUpSeconds),
                TotalDistanceMm = totalDistance
            };
        }

        private static double ComputeTrapezoidalTime(double distanceMm, double targetVelocityMmPerSec, double accelerationMmPerSec2)
        {
            double accelTime = targetVelocityMmPerSec / accelerationMmPerSec2;
            double accelDist = 0.5 * accelerationMmPerSec2 * accelTime * accelTime;

            if (2 * accelDist >= distanceMm)
            {
                // Triangle profile
                return 2.0 * Math.Sqrt(distanceMm / accelerationMmPerSec2);
            }

            double cruiseDist = distanceMm - 2 * accelDist;
            double cruiseTime = cruiseDist / targetVelocityMmPerSec;
            return 2 * accelTime + cruiseTime;
        }
    }
}
