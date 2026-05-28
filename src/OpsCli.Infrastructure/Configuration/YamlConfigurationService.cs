using OpsCli.Core.Interfaces;
using OpsCli.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpsCli.Infrastructure.Configuration;

public sealed class YamlConfigurationService : IConfigurationService
{
    public const string DefaultFileName = "opscli.yml";

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public string ResolveConfigurationPath(string? path)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            return Path.GetFullPath(path);
        }

        return Path.Combine(Directory.GetCurrentDirectory(), DefaultFileName);
    }

    public bool ConfigurationExists(string? path = null)
    {
        return File.Exists(ResolveConfigurationPath(path));
    }

    public async Task CreateSampleConfigurationAsync(string? path = null, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        var resolvedPath = ResolveConfigurationPath(path);
        if (File.Exists(resolvedPath) && !overwrite)
        {
            throw new InvalidOperationException($"Arquivo de configuracao ja existe: {resolvedPath}");
        }

        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(resolvedPath, SampleConfiguration.Content, cancellationToken);
    }

    public async Task<OpsCliConfiguration> LoadAsync(string? path = null, CancellationToken cancellationToken = default)
    {
        var resolvedPath = ResolveConfigurationPath(path);
        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException($"Arquivo de configuracao nao encontrado: {resolvedPath}", resolvedPath);
        }

        var yaml = await File.ReadAllTextAsync(resolvedPath, cancellationToken);
        var configuration = Deserializer.Deserialize<OpsCliConfiguration>(yaml) ?? new OpsCliConfiguration();
        Normalize(configuration);
        return configuration;
    }

    private static void Normalize(OpsCliConfiguration configuration)
    {
        configuration.Projects ??= new Dictionary<string, ProjectConfiguration>();
        configuration.Projects = new Dictionary<string, ProjectConfiguration>(configuration.Projects, StringComparer.OrdinalIgnoreCase);

        foreach (var project in configuration.Projects.Values)
        {
            project.Repositories ??= [];
            project.Environments ??= new Dictionary<string, EnvironmentConfiguration>();
            project.Environments = new Dictionary<string, EnvironmentConfiguration>(project.Environments, StringComparer.OrdinalIgnoreCase);

            foreach (var environment in project.Environments.Values)
            {
                environment.YamlFiles ??= [];
                environment.Urls ??= [];

                foreach (var url in environment.Urls)
                {
                    url.ExpectedStatusCodes ??= [];
                }
            }
        }
    }
}
