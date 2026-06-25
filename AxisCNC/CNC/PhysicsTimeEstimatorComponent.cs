/* Axis CNC physics-based time estimator component. */
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace AxisCNC.CNC
{
    /// <summary>
    /// Grasshopper component that estimates cut time using a simple physics model (acceleration, inertia, rapids vs cutting).
    /// </summary>
    public class PhysicsTimeEstimatorComponent : GH_Component
    {
        private const double DefaultCutFeed = 2400.0;
        private const double DefaultRapidFeed = 6000.0;
        private const double DefaultAcceleration = 1500.0;
        private const double DefaultMass = 80.0;
        private const double DefaultInertiaFactor = 0.02;
        private const double DefaultRapidThreshold = 30.0;
        private const double DefaultSpindleSpinup = 3.0;

        /// <summary>
        /// Initializes a new instance of the physics time estimator component.
        /// </summary>
        public PhysicsTimeEstimatorComponent()
          : base("Cut Time (Physics)", "TimePhysics",
              "Estimate cut time using travel distance, acceleration, mass, and rapids.",
              "Axis CNC", "CNC")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        /// <param name="pManager">The input parameter manager.</param>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Toolpath", "Pts", "Toolpath points.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Cut Feed", "CutF", "Cutting feed rate (mm/min).", GH_ParamAccess.item, DefaultCutFeed);
            pManager.AddNumberParameter("Rapid Feed", "RapidF", "Rapid feed rate (mm/min).", GH_ParamAccess.item, DefaultRapidFeed);
            pManager.AddNumberParameter("Acceleration", "Acc", "Max acceleration (mm/s^2).", GH_ParamAccess.item, DefaultAcceleration);
            pManager.AddNumberParameter("Mass", "Mass", "Moving mass (kg).", GH_ParamAccess.item, DefaultMass);
            pManager.AddNumberParameter("Inertia Factor", "Inertia", "Inertia multiplier applied to mass.", GH_ParamAccess.item, DefaultInertiaFactor);
            pManager.AddNumberParameter("Rapid Threshold", "Rapid", "Distance threshold for classifying a move as rapid (mm).", GH_ParamAccess.item, DefaultRapidThreshold);
            pManager.AddNumberParameter("Spindle Spin-up", "Spin", "Spindle spin-up/overhead time (s).", GH_ParamAccess.item, DefaultSpindleSpinup);

            for (int i = 1; i <= 7; i++)
            {
                pManager[i].Optional = true;
            }
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        /// <param name="pManager">The output parameter manager.</param>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Total (s)", "Total", "Total estimated time in seconds.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Cutting (s)", "Cut", "Cutting time in seconds.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rapids (s)", "Rapid", "Rapid move time in seconds.", GH_ParamAccess.item);
            pManager.AddTextParameter("Report", "Report", "Human-readable breakdown.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Performs the estimation.
        /// </summary>
        /// <param name="DA">The Grasshopper data access object.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> toolpath = new List<Point3d>();
            double cutFeed = DefaultCutFeed;
            double rapidFeed = DefaultRapidFeed;
            double acceleration = DefaultAcceleration;
            double mass = DefaultMass;
            double inertia = DefaultInertiaFactor;
            double rapidThreshold = DefaultRapidThreshold;
            double spinup = DefaultSpindleSpinup;

            if (!DA.GetDataList(0, toolpath)) return;
            DA.GetData(1, ref cutFeed);
            DA.GetData(2, ref rapidFeed);
            DA.GetData(3, ref acceleration);
            DA.GetData(4, ref mass);
            DA.GetData(5, ref inertia);
            DA.GetData(6, ref rapidThreshold);
            DA.GetData(7, ref spinup);

            CutPhysicsOptions options = new CutPhysicsOptions(
                cutFeed,
                rapidFeed,
                acceleration,
                mass,
                inertia,
                rapidThreshold);

            CutTimeEstimate estimate = CutTimeEstimator.EstimatePath(toolpath, options, spinup);

            DA.SetData(0, estimate.TotalSeconds);
            DA.SetData(1, estimate.CuttingSeconds);
            DA.SetData(2, estimate.RapidSeconds);
            DA.SetDataList(3, estimate.ToReport());
        }

        /// <summary>
        /// Gets the icon for this component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.Program;

        /// <summary>
        /// Gets the unique ID for this component.
        /// </summary>
        public override System.Guid ComponentGuid => new System.Guid("B0C3FDD2-3F90-4F8D-9DB9-B5832DC47B10");
    }
}
