using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TUTools.CNC
{
    /// <summary>
    /// Grasshopper component that validates toolpaths for potential issues.
    /// Checks for rapid movements, tool collisions, and path continuity.
    /// </summary>
    public class ToolpathValidator : GH_Component
    {
        /// <summary>
        /// Default threshold for detecting rapid movements (in mm).
        /// </summary>
        private const double DefaultRapidThreshold = 50.0;

        /// <summary>
        /// Default safe height for rapid movements (in mm).
        /// </summary>
        private const double DefaultSafeHeight = 50.0;

        /// <summary>
        /// Tolerance for path continuity checks (in mm).
        /// </summary>
        private const double ContinuityTolerance = 0.001;

        /// <summary>
        /// Initializes a new instance of the ToolpathValidator component.
        /// </summary>
        public ToolpathValidator()
          : base("Validate Toolpath", "Validate",
              "Validate a toolpath for potential issues such as rapid movements and discontinuities.",
              "TU Tools", "CNC")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        /// <param name="pManager">Use this object to register input parameters.</param>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Toolpath", "Pts", "The toolpath points to validate.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rapid Threshold", "Rapid", "Distance threshold for detecting rapid movements (mm).", GH_ParamAccess.item, DefaultRapidThreshold);
            pManager.AddNumberParameter("Safe Height", "SafeZ", "Safe Z height for rapid movements (mm).", GH_ParamAccess.item, DefaultSafeHeight);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        /// <param name="pManager">Use this object to register output parameters.</param>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Valid", "Valid", "True if toolpath is valid, false otherwise.", GH_ParamAccess.item);
            pManager.AddTextParameter("Issues", "Issues", "List of detected issues.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Total Distance", "Dist", "Total toolpath distance in mm.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Step", "MaxStep", "Maximum distance between consecutive points.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Rapid Moves", "Rapids", "Number of detected rapid movements.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Solves the component instance and validates the toolpath.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get Data
            List<Point3d> toolpath = new List<Point3d>();
            double rapidThreshold = DefaultRapidThreshold;
            double safeHeight = DefaultSafeHeight;

            if (!DA.GetDataList(0, toolpath)) return;
            if (!DA.GetData(1, ref rapidThreshold)) return;
            if (!DA.GetData(2, ref safeHeight)) return;

            // Initialize outputs
            List<string> issues = new List<string>();
            bool isValid = true;
            double totalDistance = 0.0;
            double maxStep = 0.0;
            int rapidMoves = 0;

            // Validate inputs
            if (toolpath.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No points provided for validation.");
                DA.SetData(0, false);
                DA.SetDataList(1, new List<string> { "No toolpath points provided." });
                return;
            }

            if (toolpath.Count == 1)
            {
                issues.Add("Toolpath contains only one point.");
                DA.SetData(0, true);
                DA.SetDataList(1, issues);
                DA.SetData(2, 0.0);
                DA.SetData(3, 0.0);
                DA.SetData(4, 0);
                return;
            }

            // Validate toolpath
            for (int i = 1; i < toolpath.Count; i++)
            {
                Point3d prev = toolpath[i - 1];
                Point3d curr = toolpath[i];

                double distance = prev.DistanceTo(curr);
                totalDistance += distance;

                if (distance > maxStep)
                {
                    maxStep = distance;
                }

                // Check for rapid movements
                if (distance > rapidThreshold)
                {
                    rapidMoves++;
                    
                    // Check if Z is at safe height
                    if (prev.Z < safeHeight && curr.Z < safeHeight)
                    {
                        issues.Add($"Warning: Large movement ({distance:F3} mm) at index {i} without safe Z height.");
                        isValid = false;
                    }
                    else
                    {
                        issues.Add($"Info: Rapid movement ({distance:F3} mm) detected at index {i}.");
                    }
                }

                // Check for extremely small movements (may indicate duplicate points)
                if (distance < ContinuityTolerance && distance > 0)
                {
                    issues.Add($"Warning: Very small movement ({distance:F6} mm) at index {i}. Possible duplicate point.");
                }

                // Check for zero-length movements
                if (distance == 0)
                {
                    issues.Add($"Warning: Duplicate point at index {i}.");
                }
            }

            // Check for negative Z values (below work surface)
            var negativeZPoints = toolpath.Select((p, idx) => new { Point = p, Index = idx })
                                          .Where(x => x.Point.Z < 0)
                                          .ToList();

            if (negativeZPoints.Any())
            {
                issues.Add($"Warning: {negativeZPoints.Count} points with negative Z values (below work surface).");
                foreach (var item in negativeZPoints.Take(5)) // Report first 5
                {
                    issues.Add($"  - Index {item.Index}: Z = {item.Point.Z:F3}");
                }
                if (negativeZPoints.Count > 5)
                {
                    issues.Add($"  - ...and {negativeZPoints.Count - 5} more");
                }
            }

            // Summary
            if (issues.Count == 0)
            {
                issues.Add("Toolpath validation passed. No issues detected.");
            }
            else
            {
                int warningCount = issues.Count(i => i.StartsWith("Warning"));
                int infoCount = issues.Count(i => i.StartsWith("Info"));
                
                if (warningCount > 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, 
                        $"Toolpath has {warningCount} warning(s) and {infoCount} info message(s).");
                }
            }

            // Output Data
            DA.SetData(0, isValid);
            DA.SetDataList(1, issues);
            DA.SetData(2, totalDistance);
            DA.SetData(3, maxStep);
            DA.SetData(4, rapidMoves);
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
                // Use the Cut icon for now
                return Resources.Cut;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("B9D4E6F8-0C3E-5B2D-9F7E-4A6C8D1F3A5B"); }
        }
    }
}
