/* Axis CNC Grasshopper assembly metadata. */
using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace AxisCNC
{
    /// <summary>
    /// Provides Grasshopper assembly metadata for Axis CNC.
    /// </summary>
    public class AxisCNCInfo : GH_AssemblyInfo
    {
        /// <summary>
        /// Gets the displayed assembly name.
        /// </summary>
        /// <returns>The assembly name shown in Grasshopper.</returns>
        public override string Name
        {
            get
            {
                return "AxisCNC";
            }
        }

        /// <summary>
        /// Gets the assembly icon.
        /// </summary>
        /// <returns>The 24x24 icon bitmap for the plugin.</returns>
        public override Bitmap Icon
        {
            get
            {
                return Resources.Main;
            }
        }

        /// <summary>
        /// Gets a short description of the assembly.
        /// </summary>
        /// <returns>The assembly description.</returns>
        public override string Description
        {
            get
            {
                return "Fabrication tools for Technical University Dublin by Axis Consulting.";
            }
        }

        /// <summary>
        /// Gets the unique identifier for the assembly.
        /// </summary>
        /// <returns>The assembly GUID.</returns>
        public override Guid Id
        {
            get
            {
                return new Guid("8dc6e4ce-f6fe-46b2-9fe1-28a56ed9edfd");
            }
        }

        /// <summary>
        /// Gets the author name.
        /// </summary>
        /// <returns>The assembly author name.</returns>
        public override string AuthorName
        {
            get
            {
                return "Axis Consulting";
            }
        }

        /// <summary>
        /// Gets the author contact information.
        /// </summary>
        /// <returns>The contact string used by Grasshopper.</returns>
        public override string AuthorContact
        {
            get
            {
                return "rhu@axisarch.tech";
            }
        }
    }
}
