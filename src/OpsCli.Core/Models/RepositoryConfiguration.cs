namespace OpsCli.Core.Models;

public sealed class RepositoryConfiguration
{
    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string DefaultBranch { get; set; } = string.Empty;
}
