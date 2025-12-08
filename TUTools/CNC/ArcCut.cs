using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TUTools.CNC
{
    /// <summary>
    /// Grasshopper component that generates G-code for circular arc movements (G02/G03).
    /// Provides smoother toolpaths for curved geometry compared to linear interpolation.
    /// </summary>
    public class ArcCut : GH_Component
    {
        /// <summary>
        /// Default feed speed in mm/s for the arc cutting operation.
        /// </summary>
        private const double DefaultFeedSpeed = 40.0;

        /// <summary>
        /// Conversion factor from mm/s to mm/min for G-code feed rate.
        /// </summary>
        private const double MM_PER_SEC_TO_MM_PER_MIN = 60.0;

        /// <summary>
        /// Number of decimal places for coordinate rounding.
        /// </summary>
        private const int CoordinatePrecision = 3;

        /// <summary>
        /// Default tolerance for arc/circle detection (in mm).
        /// </summary>
        private const double DefaultTolerance = 0.001;

        /// <summary>
        /// Initializes a new instance of the ArcCut component.
        /// </summary>
        public ArcCut()
          : base("Arc Cut", "ArcCut",
              "Create arc cutting code from circles and arcs for smoother toolpaths.",
              "TU Tools", "CNC")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        /// <param name="pManager">Use this object to register input parameters.</param>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Arcs", "Arcs", "List of circular arcs or circles to mill along.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Speed", "Speed", "Feed speed in mm/s - default 40 mm/s.", GH_ParamAccess.item, DefaultFeedSpeed);
            pManager.AddBooleanParameter("Clockwise", "CW", "Direction of arc: True for clockwise (G02), False for counterclockwise (G03).", GH_ParamAccess.item, true);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        /// <param name="pManager">Use this object to register output parameters.</param>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Code", "Code", "A list of arc cut commands (G02/G03).", GH_ParamAccess.list);
        }

        /// <summary>
        /// Solves the component instance and generates G-code arc commands from the input curves.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get Data
            List<Curve> arcs = new List<Curve>();
            double speed = DefaultFeedSpeed;
            bool clockwise = true;

            if (!DA.GetDataList(0, arcs)) return;
            if (!DA.GetData(1, ref speed)) return;
            if (!DA.GetData(2, ref clockwise)) return;

            // Validate inputs
            if (arcs.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No arcs provided for toolpath.");
                return;
            }

            if (speed <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Speed must be greater than zero.");
                return;
            }

            // Convert speed from mm/s to mm/min for G-code
            speed = speed * MM_PER_SEC_TO_MM_PER_MIN;

            // G-code command for arc direction
            string arcCommand = clockwise ? "G02" : "G03";

            // Generate arc toolpath G-code
            List<string> code = new List<string>();
            
            for (int i = 0; i < arcs.Count; i++)
            {
                Curve arc = arcs[i];

                // Get tolerance from active document, or use default if not available
                double tolerance = Rhino.RhinoDoc.ActiveDoc?.ModelAbsoluteTolerance ?? DefaultTolerance;

                // Try to convert to arc or circle
                if (!arc.TryGetArc(out Arc rhinoArc, tolerance))
                {
                    // If not an arc, try circle
                    if (!arc.TryGetCircle(out Circle circle, tolerance))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, 
                            $"Curve at index {i} is not a circular arc or circle. Skipping.");
                        continue;
                    }
                    // Convert circle to arc
                    rhinoArc = new Arc(circle, 2 * Math.PI);
                }

                // Get start and end points
                Point3d start = rhinoArc.StartPoint;
                Point3d end = rhinoArc.EndPoint;
                Point3d center = rhinoArc.Center;

                // Calculate arc center offsets (I, J, K) relative to start point
                double I = Math.Round(center.X - start.X, CoordinatePrecision);
                double J = Math.Round(center.Y - start.Y, CoordinatePrecision);
                double K = Math.Round(center.Z - start.Z, CoordinatePrecision);

                // Round coordinates
                double x = Math.Round(end.X, CoordinatePrecision);
                double y = Math.Round(end.Y, CoordinatePrecision);
                double z = Math.Round(end.Z, CoordinatePrecision);

                string cmd;

                // First move includes feed rate
                if (i == 0)
                {
                    cmd = $"{arcCommand}X{x}Y{y}Z{z}I{I}J{J}K{K}F{speed}";
                }
                else
                {
                    cmd = $"{arcCommand}X{x}Y{y}Z{z}I{I}J{J}K{K}";
                }

                code.Add(cmd);
            }

            // Output Data
            DA.SetDataList(0, code);
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
                // Use the same Cut icon for now
                return Resources.Cut;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A8C3D5E7-9B2F-4A1C-8E6D-3F5A7B9C1E2D"); }
        }
    }
}
