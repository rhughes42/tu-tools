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
    class Utilities
    {
        /// <summary>
        /// Colour a list of meshes.
        /// </summary>
        /// <param name="meshes"></param>
        /// <param name="colors"></param>
        /// <returns></returns>
        public static List<Mesh> ColorMeshes(List<Mesh> meshes, List<Color> colors)
        {
            List<Mesh> meshOut = new List<Mesh>();
            for (int i = 0; i < meshes.Count; i++)
                meshOut[i].VertexColors.CreateMonotoneMesh(colors[i]);
            return meshOut;
        }

        /// <summary>
        /// Check Rhino object types and return simple integers.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int TypeCheck(Rhino.DocObjects.RhinoObject obj)
        {
            // Check the object type and store it as a variable.
            ObjectType oType = obj.ObjectType;

            if (oType == ObjectType.Brep) { return 0; }
            else if (oType == ObjectType.Extrusion) { return 1; }
            else if (oType == ObjectType.Surface) { return 2; }
            else if (oType == ObjectType.Mesh) { return 3; }
            else if (oType == ObjectType.Curve) { return 4; }
            else if (oType == ObjectType.Point) { return 5; }

            else // If the geoemtry type is not found, return -1.
                return -1;
        }

        /// <summary>
        /// Linear interpolate between two planes.
        /// Quaternion interpolation code from the Robots plugin (https://github.com/visose/Robots)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Plane Lerp(Plane a, Plane b, double t, double min, double max)
        {
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
        /// Import some native methods for UI.
        /// </summary>
        internal sealed class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern bool AllocConsole();

            [DllImport("kernel32.dll")]
            public static extern bool FreeConsole();
        }

        /// <summary>
        /// Logic for the automatically closing message box "popup".
        /// </summary>
        public class AutoClosingMessageBox
        {
            System.Threading.Timer _timeoutTimer;
            string _caption;
            AutoClosingMessageBox(string text, string caption, int timeout)
            {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                    null, timeout, System.Threading.Timeout.Infinite);
                using (_timeoutTimer)
                    MessageBox.Show(text, caption);
            }
            public static void Show(string text, string caption, int timeout)
            {
                new AutoClosingMessageBox(text, caption, timeout);
            }
            void OnTimerElapsed(object state)
            {
                IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _timeoutTimer.Dispose();
            }
            const int WM_CLOSE = 0x0010;
            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        }
    }
}
