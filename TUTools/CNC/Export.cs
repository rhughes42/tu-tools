using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;

using static TUTools.Properties.Settings;

namespace TUTools.CNC
{
    public class Export : GH_Component
    {
        public List<string> log = new List<string>();
        public bool suppressBox = false;

        public Export()
          : base("Export", "Export",
              "Export the program as an Axiom file.",
              "TU Tools", "CNC")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Program", "Program", "List of commands to create machine file from.", GH_ParamAccess.list);
            pManager.AddTextParameter("Filename", "Filename", "Filename for the saved program file.", GH_ParamAccess.item, "CNC_File");
            pManager.AddTextParameter("Path", "Path", "Filepath to save to.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Export", "Export", "Trigger export.", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "Log", "Information log.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> program = new List<string>();
            string filename = String.Empty;
            string path = Environment.SpecialFolder.Desktop.ToString();
            bool export = false;
            bool loginValid = false;

            if (!DA.GetDataList(0, program)) return;
            if (!DA.GetData(1, ref filename)) filename = "CNC_File";
            if (!DA.GetData(2, ref path)) return;
            if (!DA.GetData(3, ref export)) return;

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
                using (StreamWriter mainProc = new StreamWriter(path + @"\" + filename + ".mmg", false))
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

                // Show a dialog box noting a successful export.
                if (!suppressBox)
                    Utilities.AutoClosingMessageBox.Show("Export Successful!", "Export", 1300);
                this.Message = "Exported";
            }
            DA.SetDataList(0, log);
        }

        // The following functions append menu items and then handle the item clicked event.
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem suppressPopup = Menu_AppendItem(menu, "No Popup", suppress_Click, true, suppressBox);
            suppressPopup.ToolTipText = "Suppress the popup notification on successful export.";
        }

        private void suppress_Click(object sender, EventArgs e)
        {
            RecordUndoEvent("SuppressClick");
            suppressBox = !suppressBox;
            ExpireSolution(true);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.Export;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("67d9291d-fb56-4517-a0fb-862cc5cd6698"); }
        }
    }
}