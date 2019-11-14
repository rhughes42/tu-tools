using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TUTools.CNC
{
    public class Program : GH_Component
    {
        public Program()
          : base("File", "File",
              "Compile a CNC program from operations.",
              "TU Tools", "CNC")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Code", "Code", "A list of cut commands.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Program", "Program", "A formatted CNC file.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get Data
            List<string> code = new List<string>();
            if (!DA.GetDataList(0, code)) return;

            // Program
            List<string> prog = new List<string>();

            prog.Add(@"( Grasshopper CNC Test )");
            prog.Add(@"( Created: " + DateTime.Now.ToShortTimeString() + ")");
            prog.Add(@"N100G00G21G17G90G40G49G80");
            prog.Add(@"N110G71G91.1");
            prog.Add(@"N120T1M06");
            prog.Add(@"N130G00G43Z50.000H1");
            prog.Add(@"N140S12000M03");
            prog.Add(@"N150G94");
            prog.Add(@"N160X0.000Y0.000F2400.0");

            for (int i = 0; i < code.Count; i++)
            {
                prog.Add("N" + ((17 + i) * 10).ToString() + code[i]);
            }

            // Hardcoded 50mm vertical set.
            prog.Add("N" + ((17 + code.Count) * 10).ToString() + "G00Z50.000");
            prog.Add("N" + ((17 + code.Count + 1) * 10).ToString() + "G00X0.000Y0.000");
            prog.Add("N" + ((17 + code.Count + 2) * 10).ToString() + "M09");
            prog.Add("N" + ((17 + code.Count + 3) * 10).ToString() + "M30");
            prog.Add("%");

            // Output Data
            DA.SetDataList(0, prog);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.Program;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("62dec010-4429-487e-8726-9758c6f6299b"); }
        }
    }
}