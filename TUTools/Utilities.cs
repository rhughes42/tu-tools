using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Rhino.DocObjects;
using Rhino.Geometry;

using static System.Math;

namespace TUTools
{
    /// <summary>
    /// Utility class containing helper methods for geometry operations and UI interactions.
    /// </summary>
    internal class Utilities
    {
        /// <summary>
        /// Colors a list of meshes with corresponding colors.
        /// </summary>
        /// <param name="meshes">The list of meshes to color.</param>
        /// <param name="colors">The list of colors to apply to each mesh.</param>
        /// <returns>A list of colored meshes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when meshes or colors is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the number of meshes and colors don't match.</exception>
        public static List<Mesh> ColorMeshes(List<Mesh> meshes, List<Color> colors)
        {
            if (meshes == null)
                throw new ArgumentNullException(nameof(meshes));
            if (colors == null)
                throw new ArgumentNullException(nameof(colors));
            if (meshes.Count != colors.Count)
                throw new ArgumentException("The number of meshes must match the number of colors.");

            List<Mesh> meshOut = new List<Mesh>();
            for (int i = 0; i < meshes.Count; i++)
            {
                Mesh mesh = meshes[i].DuplicateMesh();
                mesh.VertexColors.CreateMonotoneMesh(colors[i]);
                meshOut.Add(mesh);
            }
            return meshOut;
        }

        /// <summary>
        /// Checks Rhino object types and returns a simple integer code representing the type.
        /// </summary>
        /// <param name="obj">The Rhino object to check.</param>
        /// <returns>
        /// An integer representing the object type:
        /// 0 = Brep, 1 = Extrusion, 2 = Surface, 3 = Mesh, 4 = Curve, 5 = Point, -1 = Unknown/Unsupported
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when obj is null.</exception>
        public static int TypeCheck(Rhino.DocObjects.RhinoObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            ObjectType oType = obj.ObjectType;

            switch (oType)
            {
                case ObjectType.Brep:
                    return 0;
                case ObjectType.Extrusion:
                    return 1;
                case ObjectType.Surface:
                    return 2;
                case ObjectType.Mesh:
                    return 3;
                case ObjectType.Curve:
                    return 4;
                case ObjectType.Point:
                    return 5;
                default:
                    return -1; // If the geometry type is not found, return -1.
            }
        }

        /// <summary>
        /// Performs linear interpolation between two planes using quaternion rotation.
        /// Uses quaternion interpolation code from the Robots plugin (https://github.com/visose/Robots).
        /// </summary>
        /// <param name="a">The starting plane.</param>
        /// <param name="b">The ending plane.</param>
        /// <param name="t">The interpolation parameter value.</param>
        /// <param name="min">The minimum value for the parameter range.</param>
        /// <param name="max">The maximum value for the parameter range.</param>
        /// <returns>The interpolated plane.</returns>
        /// <exception cref="ArgumentException">Thrown when min equals max.</exception>
        public static Plane Lerp(Plane a, Plane b, double t, double min, double max)
        {
            if (min == max)
                throw new ArgumentException("Minimum and maximum values cannot be equal.");

            t = (t - min) / (max - min);
            if (double.IsNaN(t)) t = 0;
            
            var newOrigin = a.Origin * (1 - t) + b.Origin * t;

            Quaternion q = Quaternion.Rotation(a, b);
            double angle;
            Vector3d axis;
            q.GetRotation(out angle, out axis);
            angle = (angle > PI) ? angle - 2 * PI : angle;
            a.Rotate(t * angle, axis, a.Origin);

            a.Origin = newOrigin;
            return a;
        }

        /// <summary>
        /// Container for native Windows API methods for console and UI operations.
        /// </summary>
        internal sealed class NativeMethods
        {
            /// <summary>
            /// Allocates a new console for the calling process.
            /// </summary>
            /// <returns>True if the function succeeds; otherwise, false.</returns>
            [DllImport("kernel32.dll")]
            public static extern bool AllocConsole();

            /// <summary>
            /// Detaches the calling process from its console.
            /// </summary>
            /// <returns>True if the function succeeds; otherwise, false.</returns>
            [DllImport("kernel32.dll")]
            public static extern bool FreeConsole();
        }

        /// <summary>
        /// Provides functionality for displaying a message box that automatically closes after a specified timeout.
        /// </summary>
        public class AutoClosingMessageBox
        {
            private const int WM_CLOSE = 0x0010;
            private const string MessageBoxClassName = "#32770";

            private System.Threading.Timer _timeoutTimer;
            private string _caption;

            /// <summary>
            /// Initializes a new instance of the AutoClosingMessageBox class.
            /// </summary>
            /// <param name="text">The message to display in the message box.</param>
            /// <param name="caption">The title of the message box.</param>
            /// <param name="timeout">The time in milliseconds before the message box closes automatically.</param>
            private AutoClosingMessageBox(string text, string caption, int timeout)
            {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                    null, timeout, System.Threading.Timeout.Infinite);
                using (_timeoutTimer)
                    MessageBox.Show(text, caption);
            }

            /// <summary>
            /// Displays a message box that closes automatically after the specified timeout.
            /// </summary>
            /// <param name="text">The message to display in the message box.</param>
            /// <param name="caption">The title of the message box.</param>
            /// <param name="timeout">The time in milliseconds before the message box closes automatically.</param>
            public static void Show(string text, string caption, int timeout)
            {
                new AutoClosingMessageBox(text, caption, timeout);
            }

            /// <summary>
            /// Event handler called when the timer elapses.
            /// </summary>
            /// <param name="state">Timer state object (not used).</param>
            private void OnTimerElapsed(object state)
            {
                IntPtr mbWnd = FindWindow(MessageBoxClassName, _caption);
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _timeoutTimer.Dispose();
            }

            /// <summary>
            /// Retrieves a handle to the top-level window whose class name and window name match the specified strings.
            /// </summary>
            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            /// <summary>
            /// Sends a message to a specified window.
            /// </summary>
            [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        }
    }
}
