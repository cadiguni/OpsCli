using Microsoft.Extensions.DependencyInjection;
using OpsCli.Core.Interfaces;
using OpsCli.Core.Models;
using OpsCli.Core.Services;

namespace OpsCli.Cli.Commands;

internal static class CommandHelpers
{
    public static async Task<(OpsCliConfiguration? Configuration, int ExitCode)> LoadConfigurationAsync(
        IServiceProvider services,
        string? configPath,
        CancellationToken cancellationToken)
    {
        var configurationService = services.GetRequiredService<IConfigurationService>();

        try
        {
            return (await configurationService.LoadAsync(configPath, cancellationToken), 0);
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException)
        {
            Console.Error.WriteLine(ex.Message);
            return (null, 1);
        }
    }

    public static ProjectConfiguration? GetProject(OpsCliConfiguration configuration, string projectName)
    {
        var resolver = new ProjectConfigurationResolver();
        var result = resolver.GetProject(configuration, projectName);
        if (result.Success)
        {
            return result.Value;
        }

        Console.Error.WriteLine(result.Message);
        return null;
    }

    public static EnvironmentConfiguration? GetEnvironment(ProjectConfiguration project, string environmentName)
    {
        var resolver = new ProjectConfigurationResolver();
        var result = resolver.GetEnvironment(project, environmentName);
        if (result.Success)
        {
            return result.Value;
        }

        Console.Error.WriteLine(result.Message);
        return null;
    }

    public static string ResolveYamlPath(ProjectConfiguration project, string yamlFile)
    {
        if (Path.IsPathRooted(yamlFile) || project.Repositories.Count == 0)
        {
            return yamlFile;
        }

        return Path.Combine(project.Repositories[0].Path, yamlFile);
    }

    public static void PrintResult(bool success, string message)
    {
        Console.WriteLine($"{(success ? "✓" : "✗")} {message}");
    }
}
