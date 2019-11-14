using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TUTools.CNC
{
    public class Cut : GH_Component
    {
        public Cut()
          : base("Cut", "Cut",
              "Create cutting code from a list of points.",
              "TU Tools", "CNC")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Toolpath", "Pts", "A list of points to mill along.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Speed", "Speed", "Feed speed in mm/s - default 40 mm/s.", GH_ParamAccess.item, 40);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Code", "Code", "A list of cut commands.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get Data
            List<Point3d> targets = new List<Point3d>();
            double speed = 40;
            if (!DA.GetDataList(0, targets)) return;
            if (!DA.GetData(1, ref speed)) return;

            speed = speed * 60;

            // Toolpath
            List<string> code = new List<string>();
            for (int i = 0; i < targets.Count; i++)
            {
                Point3d t = targets[i];

                double x = Math.Round(t.X, 3);
                double y = Math.Round(t.Y, 3);
                double z = Math.Round(t.Z, 3);

                string cmd = String.Empty;

                if (i == 0)
                    cmd = "G1X" + x.ToString() + "Y" + y.ToString() + "Z" + z.ToString() + "F" + speed.ToString();
                else
                    cmd = "G1X" + x.ToString() + "Y" + y.ToString() + "Z" + z.ToString();

                code.Add(cmd);
            }

            // Output Data
            DA.SetDataList(0, code);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.Cut;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("76d98773-20f0-48f9-a3d8-02958efc72a3"); }
        }
    }
}