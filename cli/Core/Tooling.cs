/* Axis CNC Tooling CLI source. */
using System.Globalization;

namespace CncCli.Core;

internal readonly record struct ToolConfiguration(
    int ToolNumber,
    double Diameter,
    double Length,
    double SpindleRpm,
    double FeedRate,
    double PlungeRate,
    string Material,
    bool CoolantOn)
{
    public IEnumerable<string> ToHeader()
    {
        yield return $"( Tool {ToolNumber} | Dia {Diameter:F2}mm | {Material} )";
        yield return $"T{ToolNumber} M06";
        yield return $"S{SpindleRpm:F0} M03";
        yield return $"F{FeedRate:F1}";
        if (CoolantOn) yield return "M08";
        yield return $"( Plunge {PlungeRate:F1} mm/min )";
    }

    public string ToSummary() =>
        string.Create(CultureInfo.InvariantCulture, $"T{ToolNumber} Ø{Diameter:F1} L{Length:F0} | {SpindleRpm:F0} rpm | F{FeedRate:F0} / P{PlungeRate:F0} | {(CoolantOn ? "Coolant" : "Dry")} ({Material})");
}

internal static class CustomCommandTemplate
{
    public static string RenderSafe(string template, IEnumerable<string> pairs)
    {
        if (string.IsNullOrWhiteSpace(template)) return string.Empty;

        Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);
        foreach (string pair in pairs)
        {
            var split = pair.Split('=', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 2 && split[0].Length <= 32)
            {
                values[split[0]] = split[1];
            }
        }

        string result = template;
        foreach (var kvp in values)
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value, StringComparison.Ordinal);
        }

        // Remove any unresolved tokens to avoid accidental unsafe passthrough.
        while (true)
        {
            int start = result.IndexOf('{');
            if (start < 0) break;
            int end = result.IndexOf('}', start + 1);
            if (end < 0) break;
            result = result.Remove(start, end - start + 1);
        }

        return result.Trim();
    }
}
