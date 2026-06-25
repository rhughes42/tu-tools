/* Axis CNC tool configuration component. */
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AxisCNC.CNC
{
    /// <summary>
    /// Grasshopper component that bundles tool configuration parameters and emits helper G-code header lines.
    /// </summary>
    public class ToolConfiguratorComponent : GH_Component
    {
        private const int DefaultToolNumber = 1;
        private const double DefaultDiameter = 6.0;
        private const double DefaultLength = 50.0;
        private const double DefaultSpindle = 12000.0;
        private const double DefaultFeed = 2400.0;
        private const double DefaultPlunge = 600.0;

        /// <summary>
        /// Initializes a new instance of the tool configuration component.
        /// </summary>
        public ToolConfiguratorComponent()
          : base("Tool Configuration", "ToolCfg",
              "Define tool parameters (number, diameter, rpm, feeds) and emit helper headers.",
              "Axis CNC", "CNC")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        /// <param name="pManager">The input parameter manager.</param>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Tool Number", "T", "Tool number (T code).", GH_ParamAccess.item, DefaultToolNumber);
            pManager.AddNumberParameter("Diameter", "Dia", "Tool diameter in mm.", GH_ParamAccess.item, DefaultDiameter);
            pManager.AddNumberParameter("Length", "Len", "Tool length in mm.", GH_ParamAccess.item, DefaultLength);
            pManager.AddNumberParameter("Spindle RPM", "RPM", "Spindle speed.", GH_ParamAccess.item, DefaultSpindle);
            pManager.AddNumberParameter("Feed Rate", "Feed", "Feed rate in mm/min.", GH_ParamAccess.item, DefaultFeed);
            pManager.AddNumberParameter("Plunge Rate", "Plunge", "Plunge feed in mm/min.", GH_ParamAccess.item, DefaultPlunge);
            pManager.AddTextParameter("Material", "Mat", "Material being cut (annotation only).", GH_ParamAccess.item, "Generic");
            pManager.AddBooleanParameter("Coolant", "Cool", "Enable coolant (M08).", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        /// <param name="pManager">The output parameter manager.</param>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Summary", "Summary", "Human-readable summary.", GH_ParamAccess.item);
            pManager.AddTextParameter("Header", "Header", "Header lines for the CNC program.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Tool Config", "Cfg", "Tool configuration object.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Builds the tool configuration and header lines.
        /// </summary>
        /// <param name="DA">The Grasshopper data access object.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int toolNumber = DefaultToolNumber;
            double diameter = DefaultDiameter;
            double length = DefaultLength;
            double spindle = DefaultSpindle;
            double feed = DefaultFeed;
            double plunge = DefaultPlunge;
            string material = "Generic";
            bool coolant = false;

            DA.GetData(0, ref toolNumber);
            DA.GetData(1, ref diameter);
            DA.GetData(2, ref length);
            DA.GetData(3, ref spindle);
            DA.GetData(4, ref feed);
            DA.GetData(5, ref plunge);
            DA.GetData(6, ref material);
            DA.GetData(7, ref coolant);

            ToolConfiguration config = new ToolConfiguration(
                toolNumber,
                diameter,
                length,
                spindle,
                feed,
                plunge,
                material,
                coolant);

            IList<string> header = config.ToGCodeHeader();

            DA.SetData(0, config.ToSummary());
            DA.SetDataList(1, header);
            DA.SetData(2, config);
        }

        /// <summary>
        /// Gets the icon for this component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.Program;

        /// <summary>
        /// Gets the unique ID for this component.
        /// </summary>
        public override Guid ComponentGuid => new Guid("F2FB9CC6-6A67-4BD0-9F3C-960A9B6EA8F0");
    }
}
