/* Axis CNC Profiles CLI source. */
namespace CncCli.MachineProfiles;

internal interface IMachineProfile
{
    string Name { get; }
    IReadOnlyList<string> TransformCommands(IReadOnlyList<string> commands);
    IReadOnlyList<string> Validate(IReadOnlyList<string> commands);
}

internal sealed class GenericMachineProfile : IMachineProfile
{
    public string Name => "generic";

    public IReadOnlyList<string> TransformCommands(IReadOnlyList<string> commands) => commands;

    public IReadOnlyList<string> Validate(IReadOnlyList<string> commands) => Array.Empty<string>();
}

internal sealed class HundeggerMachineProfile : IMachineProfile
{
    public string Name => "hundegger";

    public IReadOnlyList<string> TransformCommands(IReadOnlyList<string> commands)
    {
        List<string> transformed = new(commands.Count + 1) { "(HUNDEGGER PROFILE)" };
        foreach (var cmd in commands)
        {
            string normalized = cmd.Replace("G0 ", "G00 ", StringComparison.Ordinal)
                                   .Replace("G1 ", "G01 ", StringComparison.Ordinal);
            transformed.Add(normalized);
        }
        return transformed;
    }

    public IReadOnlyList<string> Validate(IReadOnlyList<string> commands)
    {
        List<string> issues = new();
        if (!commands.Any(c => c.Contains("M03", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add("Hundegger profile expects spindle start command (M03).");
        }

        if (!commands.Any(c => c.Contains("T", StringComparison.OrdinalIgnoreCase) && c.Contains("M06", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add("Hundegger profile expects tool change command (Tn M06).");
        }

        return issues;
    }
}

internal static class MachineProfileRegistry
{
    public static IMachineProfile Resolve(string? profile)
    {
        if (string.Equals(profile, "hundegger", StringComparison.OrdinalIgnoreCase))
        {
            return new HundeggerMachineProfile();
        }

        return new GenericMachineProfile();
    }
}
