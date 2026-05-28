namespace OpsCli.Core.Models;

public sealed class EnvironmentConfiguration
{
    public List<string> YamlFiles { get; set; } = [];

    public List<UrlConfiguration> Urls { get; set; } = [];
}
