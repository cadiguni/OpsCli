namespace OpsCli.Core.Models;

public sealed class ProjectConfiguration
{
    public string Description { get; set; } = string.Empty;

    public List<RepositoryConfiguration> Repositories { get; set; } = [];

    public Dictionary<string, EnvironmentConfiguration> Environments { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
