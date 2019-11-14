using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace TUTools
{
    public class TUToolsInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "TUTools";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Resources.Main;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Fabrication tools for Technical University Dublin by Axis Consulting.";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("8dc6e4ce-f6fe-46b2-9fe1-28a56ed9edfd");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Axis Consulting";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "rhu@axisarch.tech";
            }
        }
    }
}
