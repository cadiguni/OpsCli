namespace OpsCli.Core.Models;

public sealed class OpsCliConfiguration
{
    public string Workspace { get; set; } = string.Empty;

    public Dictionary<string, ProjectConfiguration> Projects { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
