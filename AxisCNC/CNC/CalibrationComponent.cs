/* Axis CNC calibration component. */
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace AxisCNC.CNC
{
    /// <summary>
    /// Grasshopper component that applies calibration offsets (translation, rotation, scale) to a toolpath.
    /// </summary>
    public class CalibrationComponent : GH_Component
    {
        private const double DefaultRotation = 0.0;
        private const double DefaultScale = 1.0;

        /// <summary>
        /// Initializes a new instance of the calibration component.
        /// </summary>
        public CalibrationComponent()
          : base("Calibration Offsets", "Calibrate",
              "Apply calibration offsets to a toolpath (translation, rotation, scale).",
              "Axis CNC", "CNC")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        /// <param name="pManager">The input parameter manager.</param>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Toolpath", "Pts", "Points describing the toolpath.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Offset X", "X", "Translation in X (mm).", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Offset Y", "Y", "Translation in Y (mm).", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Offset Z", "Z", "Translation in Z (mm).", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Rotation", "Rot", "Rotation about origin (degrees).", GH_ParamAccess.item, DefaultRotation);
            pManager.AddNumberParameter("Scale", "Scale", "Uniform scale factor.", GH_ParamAccess.item, DefaultScale);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        /// <param name="pManager">The output parameter manager.</param>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Calibrated", "Cal", "Calibrated toolpath.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Transform", "XForm", "Applied transform.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Applies the calibration offsets to the incoming toolpath.
        /// </summary>
        /// <param name="DA">The Grasshopper data access object.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> toolpath = new List<Point3d>();
            double offsetX = 0.0;
            double offsetY = 0.0;
            double offsetZ = 0.0;
            double rotation = DefaultRotation;
            double scale = DefaultScale;

            if (!DA.GetDataList(0, toolpath)) return;
            DA.GetData(1, ref offsetX);
            DA.GetData(2, ref offsetY);
            DA.GetData(3, ref offsetZ);
            DA.GetData(4, ref rotation);
            DA.GetData(5, ref scale);

            CalibrationOffset offsets = new CalibrationOffset(offsetX, offsetY, offsetZ, rotation, scale);
            IList<Point3d> calibrated = offsets.Apply(toolpath);

            Transform transform = Transform.Scale(Point3d.Origin, scale)
                                    * Transform.Rotation(rotation * Math.PI / 180.0, Point3d.Origin)
                                    * Transform.Translation(offsetX, offsetY, offsetZ);

            DA.SetDataList(0, calibrated);
            DA.SetData(1, transform);
        }

        /// <summary>
        /// Gets the icon for this component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.Program;

        /// <summary>
        /// Gets the unique ID for this component.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5EEC8C93-3FF3-4D3D-B5AE-676F13D65B1A");
    }
}
