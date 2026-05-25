using System.Text.Json;
using CncCli.Core;

namespace CncCli.Adapters;

internal interface IExportAdapter
{
    string Name { get; }
    string Export(IReadOnlyList<CncPoint> points, IReadOnlyList<string> commands);
}

internal sealed class PlainTextExportAdapter : IExportAdapter
{
    public string Name => "plain";

    public string Export(IReadOnlyList<CncPoint> points, IReadOnlyList<string> commands)
    {
        return string.Join(Environment.NewLine, commands);
    }
}

internal sealed class JsonSimulationExportAdapter : IExportAdapter
{
    public string Name => "json";

    public string Export(IReadOnlyList<CncPoint> points, IReadOnlyList<string> commands)
    {
        return JsonSerializer.Serialize(new
        {
            points = points.Select(p => new { p.X, p.Y, p.Z }),
            commands,
            generatedUtc = DateTimeOffset.UtcNow
        });
    }
}

internal static class ExportAdapterFactory
{
    public static IExportAdapter Create(string? key)
    {
        return string.Equals(key, "json", StringComparison.OrdinalIgnoreCase)
            ? new JsonSimulationExportAdapter()
            : new PlainTextExportAdapter();
    }
}
