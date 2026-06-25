/* Axis CNC custom command component. */
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AxisCNC.CNC
{
    /// <summary>
    /// Grasshopper component that creates custom G-code commands using a tokenized template.
    /// </summary>
    public class CustomCommandComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the custom command component.
        /// </summary>
        public CustomCommandComponent()
          : base("Custom Command", "Command",
              "Create custom commands using a template and key/value pairs.",
              "Axis CNC", "CNC")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        /// <param name="pManager">The input parameter manager.</param>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name of the custom command.", GH_ParamAccess.item, "Custom");
            pManager.AddTextParameter("Template", "Template", "Template with tokens like {X}, {Y}, {Feed}.", GH_ParamAccess.item, "G1 X{X} Y{Y} Z{Z} F{Feed}");
            pManager.AddTextParameter("Pairs", "Pairs", "Key=value pairs to replace tokens (one per line).", GH_ParamAccess.list);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        /// <param name="pManager">The output parameter manager.</param>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Command", "Cmd", "Expanded command.", GH_ParamAccess.item);
            pManager.AddTextParameter("Definition", "Def", "Definition echo with defaults.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Renders the custom command string.
        /// </summary>
        /// <param name="DA">The Grasshopper data access object.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "Custom";
            string template = "G1 X{X} Y{Y} Z{Z} F{Feed}";
            List<string> pairs = new List<string>();

            DA.GetData(0, ref name);
            DA.GetData(1, ref template);
            DA.GetDataList(2, pairs);

            Dictionary<string, string> parameters = ParsePairs(pairs);
            CustomCommandDefinition def = new CustomCommandDefinition(name, template, parameters);

            string command = def.Render();
            string definition = $"{def.Name}: {def.Template}";

            DA.SetData(0, command);
            DA.SetData(1, definition);
        }

        /// <summary>
        /// Parses key-value pairs in the form <c>key=value</c>.
        /// </summary>
        /// <param name="pairs">The input lines to parse.</param>
        /// <returns>A case-insensitive dictionary of parameter values.</returns>
        private static Dictionary<string, string> ParsePairs(IEnumerable<string> pairs)
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (pairs == null) return map;

            foreach (string raw in pairs)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                int idx = raw.IndexOf('=');
                if (idx <= 0 || idx >= raw.Length - 1) continue;

                string key = raw.Substring(0, idx).Trim();
                string value = raw.Substring(idx + 1).Trim();

                if (!string.IsNullOrWhiteSpace(key))
                {
                    map[key] = value;
                }
            }

            return map;
        }

        /// <summary>
        /// Gets the icon for this component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.Program;

        /// <summary>
        /// Gets the unique ID for this component.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0EB1DF4D-5913-4E38-9B5C-7A7C20E3B9C2");
    }
}
