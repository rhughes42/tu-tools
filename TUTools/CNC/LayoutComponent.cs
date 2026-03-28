using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TUTools.CNC
{
    /// <summary>
    /// Grasshopper component that generates layout variations (grid, mirrors, rotations) for toolpaths.
    /// </summary>
    public class LayoutComponent : GH_Component
    {
        private const int DefaultRows = 1;
        private const int DefaultColumns = 1;
        private const double DefaultSpacing = 50.0;

        public LayoutComponent()
          : base("Layout Planner", "Layout",
              "Replicate toolpaths using grid, mirror, or rotation strategies.",
              "TU Tools", "CNC")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Toolpath", "Pts", "Base toolpath points.", GH_ParamAccess.list);
            pManager.AddTextParameter("Strategy", "Strategy", "Layout strategy: Grid, MirrorX, MirrorY, Rotate90, Rotate180, Rotate270.", GH_ParamAccess.item, "Grid");
            pManager.AddIntegerParameter("Rows", "Rows", "Grid rows (for Grid strategy).", GH_ParamAccess.item, DefaultRows);
            pManager.AddIntegerParameter("Columns", "Cols", "Grid columns (for Grid strategy).", GH_ParamAccess.item, DefaultColumns);
            pManager.AddNumberParameter("Spacing X", "SX", "Spacing in X (mm) for grid layout.", GH_ParamAccess.item, DefaultSpacing);
            pManager.AddNumberParameter("Spacing Y", "SY", "Spacing in Y (mm) for grid layout.", GH_ParamAccess.item, DefaultSpacing);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Layout", "Layout", "Combined layout toolpath.", GH_ParamAccess.list);
            pManager.AddTextParameter("Info", "Info", "Description of generated layout.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Generates the requested layout.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> toolpath = new List<Point3d>();
            string strategyText = "Grid";
            int rows = DefaultRows;
            int cols = DefaultColumns;
            double spacingX = DefaultSpacing;
            double spacingY = DefaultSpacing;

            if (!DA.GetDataList(0, toolpath)) return;
            DA.GetData(1, ref strategyText);
            DA.GetData(2, ref rows);
            DA.GetData(3, ref cols);
            DA.GetData(4, ref spacingX);
            DA.GetData(5, ref spacingY);

            LayoutStrategy strategy = ParseStrategy(strategyText);
            LayoutOptions options = new LayoutOptions(strategy, rows, cols, spacingX, spacingY);

            IList<Point3d> layout = options.Apply(toolpath);
            List<string> info = new List<string>
            {
                $"Strategy: {strategy}",
                $"Rows: {options.Rows}, Columns: {options.Columns}",
                $"Spacing: {options.SpacingX:F1}mm x {options.SpacingY:F1}mm",
                $"Points generated: {layout.Count}"
            };

            DA.SetDataList(0, layout);
            DA.SetDataList(1, info);
        }

        private static LayoutStrategy ParseStrategy(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return LayoutStrategy.None;

            string normalized = text.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "grid": return LayoutStrategy.Grid;
                case "mirrorx": return LayoutStrategy.MirrorX;
                case "mirrory": return LayoutStrategy.MirrorY;
                case "rotate90": return LayoutStrategy.Rotate90;
                case "rotate180": return LayoutStrategy.Rotate180;
                case "rotate270": return LayoutStrategy.Rotate270;
                default: return LayoutStrategy.None;
            }
        }

        /// <summary>
        /// Gets the icon for this component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.Program;

        /// <summary>
        /// Gets the unique ID for this component.
        /// </summary>
        public override Guid ComponentGuid => new Guid("C8E50B80-5D20-4FB9-B96E-17E2D0F1F6FA");
    }
}
