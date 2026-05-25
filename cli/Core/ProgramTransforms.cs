namespace CncCli.Core;

internal static class ProgramTransforms
{
    public static IReadOnlyList<string> NormalizeCommands(IEnumerable<string> commands)
    {
        return commands
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<string> AddLineNumbers(IEnumerable<string> commands, int start = 10, int step = 10)
    {
        int line = start;
        List<string> numbered = new();
        foreach (string cmd in commands)
        {
            numbered.Add($"N{line} {cmd}");
            line += step;
        }
        return numbered;
    }
}
