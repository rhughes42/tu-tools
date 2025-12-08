using System;
using System.Collections.Generic;

using Grasshopper.Kernel;

using static TUTools.Properties.Settings;

namespace TUTools.CNC
{
    /// <summary>
    /// Grasshopper component that compiles individual CNC operations into a complete CNC program file.
    /// Adds standard header, initialization, and footer commands required for machine operation.
    /// </summary>
    public class Program : GH_Component
    {
        /// <summary>
        /// Safe height for rapid movements (Z-axis).
        /// </summary>
        private const double SafeHeight = 50.0;

        /// <summary>
        /// Starting line number for the G-code program.
        /// </summary>
        private const int StartLineNumber = 170;

        /// <summary>
        /// Increment for line numbers in the G-code program.
        /// </summary>
        private const int LineNumberIncrement = 10;

        /// <summary>
        /// Initializes a new instance of the Program component.
        /// </summary>
        public Program()
          : base("File", "File",
              "Compile a CNC program from operations.",
              "TU Tools", "CNC")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        /// <param name="pManager">Use this object to register input parameters.</param>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Code", "Code", "A list of cut commands.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        /// <param name="pManager">Use this object to register output parameters.</param>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Program", "Program", "A formatted CNC file.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Solves the component instance and compiles a complete CNC program.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get Data
            List<string> code = new List<string>();
            if (!DA.GetDataList(0, code)) return;

            bool loginValid = false;

            // Program
            List<string> prog = new List<string>();

            // Check the license status
            this.Message = "OK";
            if (Default.ValidTo.CompareTo(DateTime.Now) <= 0)
            {
                Default.LoggedIn = false;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Log in to Axis to use this feature.");
                this.Message = "Timed Out";
            }
            else loginValid = true;

            if (loginValid)
            {
                // Add header and initialization commands
                prog.Add("( Grasshopper CNC Test )");
                prog.Add("( Toolpath Created: " + DateTime.Now.ToShortTimeString() + " )");
                prog.Add("( --- )");
                prog.Add("N100G00G21G17G90G40G49G80");  // Initialize machine: rapid positioning, metric units, XY plane, absolute mode
                prog.Add("N110G71G91.1");                // Arc center mode
                prog.Add("N120T1M06");                   // Tool change to tool 1
                prog.Add("N130G00G43Z" + SafeHeight.ToString("F3") + "H1");  // Rapid to safe height with tool length compensation
                prog.Add("N140S12000M03");               // Spindle on clockwise at 12000 RPM
                prog.Add("N150G94");                     // Feed rate mode: units per minute
                prog.Add("N160X0.000Y0.000F2400.0");     // Move to origin

                // Add user-provided code
                for (int i = 0; i < code.Count; i++)
                {
                    int lineNumber = (StartLineNumber / LineNumberIncrement + i) * LineNumberIncrement;
                    prog.Add($"N{lineNumber}{code[i]}");
                }

                // Add footer commands
                int finalLineBase = (StartLineNumber / LineNumberIncrement + code.Count) * LineNumberIncrement;
                prog.Add($"N{finalLineBase}G00Z{SafeHeight:F3}");      // Rapid to safe height
                prog.Add($"N{finalLineBase + LineNumberIncrement}G00X0.000Y0.000");  // Return to origin
                prog.Add($"N{finalLineBase + 2 * LineNumberIncrement}M09");          // Coolant off
                prog.Add($"N{finalLineBase + 3 * LineNumberIncrement}M30");          // Program end
                prog.Add("%");                                                        // End of program marker
            }

            // Output Data
            DA.SetDataList(0, prog);
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
            get { return new Guid("62dec010-4429-487e-8726-9758c6f6299b"); }
        }
    }
}