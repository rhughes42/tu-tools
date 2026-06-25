using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace AxisCNC.CNC
{
    /// <summary>
    /// Grasshopper component that visualizes G-code toolpaths with color-coded movements.
    /// Displays cutting moves, rapid moves, and provides statistics about the toolpath.
    /// </summary>
    public class GCodePreview : GH_Component
    {
        /// <summary>
        /// Color for cutting movements (green).
        /// </summary>
        private static readonly Color CuttingColor = Color.FromArgb(0, 255, 0);

        /// <summary>
        /// Color for rapid movements (red).
        /// </summary>
        private static readonly Color RapidColor = Color.FromArgb(255, 0, 0);

        /// <summary>
        /// Color for arc movements (blue).
        /// </summary>
        private static readonly Color ArcColor = Color.FromArgb(0, 100, 255);

        /// <summary>
        /// Threshold for classifying movements as rapid (in mm).
        /// </summary>
        private const double RapidThreshold = 30.0;

        /// <summary>
        /// Default average feed rate for time estimation (mm/s).
        /// </summary>
        private const double DefaultFeedRate = 40.0;

        /// <summary>
        /// Speed multiplier for rapid movements.
        /// </summary>
        private const double RapidSpeedMultiplier = 3.0;

        /// <summary>
        /// Cached toolpath data to avoid recomputation on every redraw.
        /// </summary>
        private List<Point3d> _cachedToolpath = null;
        private int _cachedDataVersion = -1;

        /// <summary>
        /// Initializes a new instance of the GCodePreview component.
        /// </summary>
        public GCodePreview()
          : base("G-Code Preview", "Preview",
              "Visualize G-code toolpaths with color-coded cutting and rapid movements.",
              "Axis CNC", "CNC")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        /// <param name="pManager">Use this object to register input parameters.</param>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Toolpath", "Pts", "The toolpath points to preview.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rapid Threshold", "Rapid", "Distance threshold for rapid movements (mm).", GH_ParamAccess.item, RapidThreshold);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        /// <param name="pManager">Use this object to register output parameters.</param>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Cutting Moves", "Cut", "Lines representing cutting movements.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Rapid Moves", "Rapid", "Lines representing rapid movements.", GH_ParamAccess.list);
            pManager.AddTextParameter("Statistics", "Stats", "Toolpath statistics.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Solves the component instance and generates preview geometry.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get Data
            List<Point3d> toolpath = new List<Point3d>();
            double rapidThreshold = RapidThreshold;

            if (!DA.GetDataList(0, toolpath)) return;
            if (!DA.GetData(1, ref rapidThreshold)) return;

            // Initialize outputs
            List<Line> cuttingMoves = new List<Line>();
            List<Line> rapidMoves = new List<Line>();
            List<string> statistics = new List<string>();

            // Validate inputs
            if (toolpath.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No points provided for preview.");
                return;
            }

            if (toolpath.Count == 1)
            {
                statistics.Add("Single point toolpath.");
                DA.SetDataList(0, cuttingMoves);
                DA.SetDataList(1, rapidMoves);
                DA.SetDataList(2, statistics);
                return;
            }

            // Analyze toolpath
            double totalCuttingDistance = 0.0;
            double totalRapidDistance = 0.0;
            int cuttingMoveCount = 0;
            int rapidMoveCount = 0;

            for (int i = 1; i < toolpath.Count; i++)
            {
                Point3d start = toolpath[i - 1];
                Point3d end = toolpath[i];
                double distance = start.DistanceTo(end);

                Line segment = new Line(start, end);

                // Classify as cutting or rapid movement
                if (distance > rapidThreshold)
                {
                    rapidMoves.Add(segment);
                    rapidMoveCount++;
                    totalRapidDistance += distance;
                }
                else
                {
                    cuttingMoves.Add(segment);
                    cuttingMoveCount++;
                    totalCuttingDistance += distance;
                }
            }

            // Calculate statistics
            double totalDistance = totalCuttingDistance + totalRapidDistance;
            
            // Get bounding box
            BoundingBox bbox = new BoundingBox(toolpath);
            double workspaceX = bbox.Max.X - bbox.Min.X;
            double workspaceY = bbox.Max.Y - bbox.Min.Y;
            double workspaceZ = bbox.Max.Z - bbox.Min.Z;

            // Generate statistics
            statistics.Add($"=== Toolpath Statistics ===");
            statistics.Add($"Total Points: {toolpath.Count}");
            statistics.Add($"Total Distance: {totalDistance:F3} mm");
            statistics.Add($"");
            statistics.Add($"Cutting Moves: {cuttingMoveCount}");
            statistics.Add($"Cutting Distance: {totalCuttingDistance:F3} mm ({(totalCuttingDistance / totalDistance * 100):F1}%)");
            statistics.Add($"");
            statistics.Add($"Rapid Moves: {rapidMoveCount}");
            statistics.Add($"Rapid Distance: {totalRapidDistance:F3} mm ({(totalRapidDistance / totalDistance * 100):F1}%)");
            statistics.Add($"");
            statistics.Add($"Workspace Dimensions:");
            statistics.Add($"  X: {workspaceX:F3} mm");
            statistics.Add($"  Y: {workspaceY:F3} mm");
            statistics.Add($"  Z: {workspaceZ:F3} mm");
            statistics.Add($"");
            statistics.Add($"Z Range: {bbox.Min.Z:F3} to {bbox.Max.Z:F3} mm");

            // Estimate cutting time
            double cuttingTime = totalCuttingDistance / DefaultFeedRate;
            double rapidTime = totalRapidDistance / (DefaultFeedRate * RapidSpeedMultiplier);
            double totalTime = cuttingTime + rapidTime;

            statistics.Add($"");
            statistics.Add($"Estimated Time (at {DefaultFeedRate} mm/s feed):");
            statistics.Add($"  Cutting: {FormatTime(cuttingTime)}");
            statistics.Add($"  Rapids: {FormatTime(rapidTime)}");
            statistics.Add($"  Total: {FormatTime(totalTime)}");

            // Output Data
            DA.SetDataList(0, cuttingMoves);
            DA.SetDataList(1, rapidMoves);
            DA.SetDataList(2, statistics);
        }

        /// <summary>
        /// Formats time in seconds to a human-readable string.
        /// </summary>
        /// <param name="seconds">Time in seconds.</param>
        /// <returns>Formatted time string.</returns>
        private string FormatTime(double seconds)
        {
            if (seconds < 60)
                return $"{seconds:F1}s";
            else if (seconds < 3600)
            {
                int mins = (int)(seconds / 60);
                int secs = (int)(seconds % 60);
                return $"{mins}m {secs}s";
            }
            else
            {
                int hours = (int)(seconds / 3600);
                int mins = (int)((seconds % 3600) / 60);
                return $"{hours}h {mins}m";
            }
        }

        /// <summary>
        /// Custom preview display for color-coded toolpath visualization.
        /// </summary>
        /// <param name="args">Preview display arguments.</param>
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);

            if (!m_data.IsValid)
                return;

            // Get the toolpath from the first input
            var toolpathParam = Params.Input[0];
            if (toolpathParam.VolatileDataCount == 0)
                return;

            // Check if we need to update cached data
            int currentVersion = toolpathParam.VolatileData.DataCount;
            if (_cachedToolpath == null || _cachedDataVersion != currentVersion)
            {
                _cachedToolpath = new List<Point3d>();
                foreach (var data in toolpathParam.VolatileData.AllData(true))
                {
                    if (data is Grasshopper.Kernel.Types.GH_Point ghPoint)
                    {
                        _cachedToolpath.Add(ghPoint.Value);
                    }
                }
                _cachedDataVersion = currentVersion;
            }

            if (_cachedToolpath.Count < 2)
                return;

            double rapidThreshold = RapidThreshold;
            if (Params.Input[1].VolatileDataCount > 0)
            {
                var thresholdData = Params.Input[1].VolatileData.get_Branch(0)[0];
                if (thresholdData is Grasshopper.Kernel.Types.GH_Number ghNumber)
                {
                    rapidThreshold = ghNumber.Value;
                }
            }

            // Draw colored lines
            for (int i = 1; i < _cachedToolpath.Count; i++)
            {
                Point3d start = _cachedToolpath[i - 1];
                Point3d end = _cachedToolpath[i];
                double distance = start.DistanceTo(end);

                Color lineColor = distance > rapidThreshold ? RapidColor : CuttingColor;
                args.Display.DrawLine(start, end, lineColor, 2);
            }

            // Draw start and end markers
            if (_cachedToolpath.Count > 0)
            {
                args.Display.DrawPoint(_cachedToolpath[0], Rhino.Display.PointStyle.X, 10, Color.Blue);
                args.Display.DrawPoint(_cachedToolpath[_cachedToolpath.Count - 1], Rhino.Display.PointStyle.Circle, 10, Color.Red);
            }
        }

        /// <summary>
        /// Gets the exposure of this component in the component category.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Gets the icon for this component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.Program;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C1E5F9A3-2D4B-6E8C-A0F2-5B7D9E3C6A8F"); }
        }
    }
}
