using Microsoft.Extensions.DependencyInjection;
using OpsCli.Core.Interfaces;
using OpsCli.Core.Models;

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
        if (configuration.Projects.TryGetValue(projectName, out var project))
        {
            return project;
        }

        Console.Error.WriteLine($"Projeto nao encontrado: {projectName}");
        return null;
    }

    public static EnvironmentConfiguration? GetEnvironment(ProjectConfiguration project, string environmentName)
    {
        if (project.Environments.TryGetValue(environmentName, out var environment))
        {
            return environment;
        }

        Console.Error.WriteLine($"Ambiente nao encontrado: {environmentName}");
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
        Console.WriteLine($"{(success ? "[OK]" : "[FAIL]")} {message}");
    }
}
