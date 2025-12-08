using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TUTools.CNC
{
    /// <summary>
    /// Grasshopper component that generates CNC cutting code (G-code) from a list of points.
    /// </summary>
    public class Cut : GH_Component
    {
        /// <summary>
        /// Default feed speed in mm/s for the cutting operation.
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
        /// Initializes a new instance of the Cut component.
        /// </summary>
        public Cut()
          : base("Cut", "Cut",
              "Create cutting code from a list of points.",
              "TU Tools", "CNC")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        /// <param name="pManager">Use this object to register input parameters.</param>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Toolpath", "Pts", "A list of points to mill along.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Speed", "Speed", "Feed speed in mm/s - default 40 mm/s.", GH_ParamAccess.item, DefaultFeedSpeed);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        /// <param name="pManager">Use this object to register output parameters.</param>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Code", "Code", "A list of cut commands.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Solves the component instance and generates G-code commands from the input points.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get Data
            List<Point3d> targets = new List<Point3d>();
            double speed = DefaultFeedSpeed;
            if (!DA.GetDataList(0, targets)) return;
            if (!DA.GetData(1, ref speed)) return;

            // Validate inputs
            if (targets.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No points provided for toolpath.");
                return;
            }

            if (speed <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Speed must be greater than zero.");
                return;
            }

            // Convert speed from mm/s to mm/min for G-code
            speed = speed * MM_PER_SEC_TO_MM_PER_MIN;

            // Generate toolpath G-code
            List<string> code = new List<string>();
            for (int i = 0; i < targets.Count; i++)
            {
                Point3d t = targets[i];

                double x = Math.Round(t.X, CoordinatePrecision);
                double y = Math.Round(t.Y, CoordinatePrecision);
                double z = Math.Round(t.Z, CoordinatePrecision);

                string cmd;

                // First move includes feed rate
                if (i == 0)
                    cmd = $"G1X{x}Y{y}Z{z}F{speed}";
                else
                    cmd = $"G1X{x}Y{y}Z{z}";

                code.Add(cmd);
            }

            // Output Data
            DA.SetDataList(0, code);
        }

        /// <summary>
        /// Gets the icon for this component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.Cut;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("76d98773-20f0-48f9-a3d8-02958efc72a3"); }
        }
    }
}