using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TUTools.CNC
{
    /// <summary>
    /// Grasshopper component that shortens and simplifies toolpaths using tolerance filtering,
    /// nearest-neighbor ordering, and 2-opt refinement.
    /// </summary>
    public class OptimizeToolpath : GH_Component
    {
        private const double DefaultTolerance = 0.05;
        private const int DefaultIterations = 50;
        private const double TwoOptEpsilon = 1e-6;

        public OptimizeToolpath()
          : base("Optimize Toolpath", "Optimize",
              "Reduce small moves and optionally reorder points to shorten travel distance.",
              "TU Tools", "CNC")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Toolpath", "Pts", "Points describing the toolpath polyline.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "Tol", "Minimum move (mm) to keep a point. Smaller steps are removed.", GH_ParamAccess.item, DefaultTolerance);
            pManager.AddBooleanParameter("Optimize Order", "Order", "Reorder points with nearest-neighbor + 2-opt heuristics.", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Iterations", "Iter", "Maximum 2-opt improvement passes.", GH_ParamAccess.item, DefaultIterations);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Optimized", "Opt", "Optimized toolpath.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Removed Points", "Removed", "Number of points removed by tolerance filtering.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Original Length", "L0", "Original polyline length in mm.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Optimized Length", "L1", "Optimized polyline length in mm.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Improvement (%)", "Delta%", "Relative reduction ((L0 - L1) / L0 * 100).", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> toolpath = new List<Point3d>();
            double tolerance = DefaultTolerance;
            bool optimizeOrder = false;
            int iterations = DefaultIterations;

            if (!DA.GetDataList(0, toolpath)) return;
            DA.GetData(1, ref tolerance);
            DA.GetData(2, ref optimizeOrder);
            DA.GetData(3, ref iterations);

            if (toolpath.Count < 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Provide at least two points to optimize a toolpath.");
                DA.SetDataList(0, toolpath);
                DA.SetData(1, 0);
                DA.SetData(2, 0.0);
                DA.SetData(3, 0.0);
                DA.SetData(4, 0.0);
                return;
            }

            tolerance = Math.Max(0.0, tolerance);
            iterations = Math.Max(0, iterations);

            double originalLength = PathLength(toolpath);

            List<Point3d> filtered = ReduceNoise(toolpath, tolerance, out int removedCount);
            List<Point3d> optimized = filtered;

            if (optimizeOrder && filtered.Count > 2)
            {
                optimized = ImproveOrder(filtered, iterations);
            }

            double optimizedLength = PathLength(optimized);
            double improvement = originalLength > 0
                ? (originalLength - optimizedLength) / originalLength * 100.0
                : 0.0;

            DA.SetDataList(0, optimized);
            DA.SetData(1, removedCount);
            DA.SetData(2, originalLength);
            DA.SetData(3, optimizedLength);
            DA.SetData(4, improvement);
        }

        private static List<Point3d> ReduceNoise(IReadOnlyList<Point3d> path, double tolerance, out int removedCount)
        {
            removedCount = 0;
            List<Point3d> filtered = new List<Point3d>();
            if (path.Count == 0) return filtered;

            filtered.Add(path[0]);
            Point3d last = path[0];

            for (int i = 1; i < path.Count; i++)
            {
                Point3d candidate = path[i];
                double distance = last.DistanceTo(candidate);

                if (distance >= tolerance)
                {
                    filtered.Add(candidate);
                    last = candidate;
                }
                else
                {
                    removedCount++;
                }
            }

            return filtered;
        }

        private static List<Point3d> ImproveOrder(IReadOnlyList<Point3d> path, int maxIterations)
        {
            List<Point3d> ordered = NearestNeighbor(path);
            TwoOpt(ordered, maxIterations);
            return ordered;
        }

        private static List<Point3d> NearestNeighbor(IReadOnlyList<Point3d> path)
        {
            List<Point3d> remaining = new List<Point3d>(path.Count);
            for (int i = 1; i < path.Count; i++)
            {
                remaining.Add(path[i]);
            }

            List<Point3d> ordered = new List<Point3d> { path[0] };
            Point3d current = path[0];

            while (remaining.Count > 0)
            {
                double bestDistance = double.MaxValue;
                int bestIndex = 0;

                for (int i = 0; i < remaining.Count; i++)
                {
                    double distance = current.DistanceTo(remaining[i]);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestIndex = i;
                    }
                }

                current = remaining[bestIndex];
                ordered.Add(current);
                remaining.RemoveAt(bestIndex);
            }

            return ordered;
        }

        private static void TwoOpt(List<Point3d> path, int maxIterations)
        {
            if (path.Count < 4 || maxIterations == 0) return;

            bool improved = true;
            int iteration = 0;

            while (improved && iteration < maxIterations)
            {
                improved = false;
                iteration++;

                for (int i = 1; i < path.Count - 2; i++)
                {
                    for (int j = i + 1; j < path.Count - 1; j++)
                    {
                        double delta = DeltaTwoOpt(path, i, j);
                        if (delta < -TwoOptEpsilon)
                        {
                            ReverseSegment(path, i, j);
                            improved = true;
                        }
                    }
                }
            }
        }

        private static double DeltaTwoOpt(IReadOnlyList<Point3d> path, int i, int j)
        {
            Point3d a = path[i - 1];
            Point3d b = path[i];
            Point3d c = path[j];
            Point3d d = path[j + 1];

            double before = a.DistanceTo(b) + c.DistanceTo(d);
            double after = a.DistanceTo(c) + b.DistanceTo(d);

            return after - before;
        }

        private static void ReverseSegment(List<Point3d> path, int start, int end)
        {
            while (start < end)
            {
                Point3d temp = path[start];
                path[start] = path[end];
                path[end] = temp;
                start++;
                end--;
            }
        }

        private static double PathLength(IReadOnlyList<Point3d> path)
        {
            double length = 0.0;
            for (int i = 1; i < path.Count; i++)
            {
                length += path[i - 1].DistanceTo(path[i]);
            }

            return length;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Resources.Cut; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("D929F768-4B87-4DF1-9D16-2B8A49F2A3F1"); }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }
    }
}
