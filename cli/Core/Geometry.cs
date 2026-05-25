namespace CncCli.Core;

internal readonly record struct CncPoint(double X, double Y, double Z)
{
    public double DistanceTo(CncPoint other)
    {
        double dx = X - other.X;
        double dy = Y - other.Y;
        double dz = Z - other.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
