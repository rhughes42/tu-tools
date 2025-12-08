using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;

using static TUTools.Properties.Settings;

namespace TUTools.CNC
{
    /// <summary>
    /// Grasshopper component that exports CNC programs to Axiom machine file format (.mmg).
    /// Requires authentication via the Login component.
    /// </summary>
    public class Export : GH_Component
    {
        /// <summary>
        /// Default filename for exported files.
        /// </summary>
        private const string DefaultFilename = "CNC_File";

        /// <summary>
        /// File extension for machine files.
        /// </summary>
        private const string FileExtension = ".mmg";

        /// <summary>
        /// Duration in milliseconds for the success message popup.
        /// </summary>
        private const int PopupDuration = 1300;

        /// <summary>
        /// Log of export operations.
        /// </summary>
        public List<string> log = new List<string>();

        /// <summary>
        /// Flag to suppress the success message popup.
        /// </summary>
        public bool suppressBox = false;

        /// <summary>
        /// Initializes a new instance of the Export component.
        /// </summary>
        public Export()
          : base("Export", "Export",
              "Export the program as an Axiom file.",
              "TU Tools", "CNC")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        /// <param name="pManager">Use this object to register input parameters.</param>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Program", "Program", "List of commands to create machine file from.", GH_ParamAccess.list);
            pManager.AddTextParameter("Filename", "Filename", "Filename for the saved program file.", GH_ParamAccess.item, DefaultFilename);
            pManager.AddTextParameter("Path", "Path", "Filepath to save to.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Export", "Export", "Trigger export.", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        /// <param name="pManager">Use this object to register output parameters.</param>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "Log", "Information log.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Solves the component instance and exports the CNC program to a file.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> program = new List<string>();
            string filename = DefaultFilename;
            string path = string.Empty;
            bool export = false;
            bool loginValid = false;

            if (!DA.GetDataList(0, program)) return;
            if (!DA.GetData(1, ref filename)) filename = DefaultFilename;
            if (!DA.GetData(2, ref path)) return;
            if (!DA.GetData(3, ref export)) return;

            // Validate inputs
            if (program.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No program commands provided.");
                return;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid file path provided.");
                return;
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = DefaultFilename;
            }

            // Check the license status
            this.Message = "OK";
            if (Default.ValidTo.CompareTo(DateTime.Now) <= 0)
            {
                Default.LoggedIn = false;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Log in to Axis to use this feature.");
                log.Add("Login error at " + DateTime.Now.ToShortTimeString());
                this.Message = "Timed Out";
            }
            else loginValid = true;

            if (export && loginValid)
            {
                try
                {
                    string fullPath = Path.Combine(path, filename + FileExtension);
                    
                    using (StreamWriter mainProc = new StreamWriter(fullPath, false))
                    {
                        mainProc.WriteLine("( TU Tools )");
                        mainProc.WriteLine("( Axis Consulting for Technical University Dublin )");
                        mainProc.WriteLine("( Contact rhu@axisarch.tech )");
                        mainProc.WriteLine("( --- )");
                        mainProc.WriteLine("( Exported " + DateTime.Now.ToShortTimeString() + " by " + Environment.MachineName + " )");
                        mainProc.WriteLine("( --- )");
                        for (int i = 0; i < program.Count; i++)
                        {
                            mainProc.WriteLine(program[i]);
                        }
                    }

                    log.Add("Exported " + filename + " at " + DateTime.Now.ToShortTimeString());

                    // Show a dialog box noting a successful export
                    if (!suppressBox)
                        Utilities.AutoClosingMessageBox.Show("Export Successful!", "Export", PopupDuration);
                    this.Message = "Exported";
                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to export file: " + ex.Message);
                    log.Add("Export failed: " + ex.Message);
                }
            }
            DA.SetDataList(0, log);
        }

        /// <summary>
        /// Appends additional menu items to the component's context menu.
        /// </summary>
        /// <param name="menu">The menu to append items to.</param>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem suppressPopup = Menu_AppendItem(menu, "No Popup", suppress_Click, true, suppressBox);
            suppressPopup.ToolTipText = "Suppress the popup notification on successful export.";
        }

        /// <summary>
        /// Handles the click event for the suppress popup menu item.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void suppress_Click(object sender, EventArgs e)
        {
            RecordUndoEvent("SuppressClick");
            suppressBox = !suppressBox;
            ExpireSolution(true);
        }

        /// <summary>
        /// Gets the icon for this component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.Export;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("67d9291d-fb56-4517-a0fb-862cc5cd6698"); }
        }
    }
}