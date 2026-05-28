using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using OpsCli.Core.Interfaces;

namespace OpsCli.Cli.Commands;

public static class UrlCommands
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("urls", "Consulta URLs configuradas por ambiente.");
        command.Add(CreateListCommand(services));
        command.Add(CreateCheckCommand(services));
        return command;
    }

    private static Command CreateListCommand(IServiceProvider services)
    {
        var projectOption = ProjectOption();
        var envOption = EnvOption();
        var configOption = ConfigOption();
        var command = new Command("list", "Lista URLs cadastradas para um ambiente.");
        command.Add(projectOption);
        command.Add(envOption);
        command.Add(configOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var environment = await LoadEnvironmentAsync(services, parseResult.GetRequiredValue(projectOption), parseResult.GetRequiredValue(envOption), parseResult.GetValue(configOption), cancellationToken);
            if (environment is null)
            {
                return 1;
            }

            Console.WriteLine("URLs configuradas:");
            foreach (var url in environment.Urls)
            {
                Console.WriteLine($"- {url.Name}: {url.Url} (esperado: {string.Join(", ", url.ExpectedStatusCodes)})");
            }

            return 0;
        });
        return command;
    }

    private static Command CreateCheckCommand(IServiceProvider services)
    {
        var projectOption = ProjectOption();
        var envOption = EnvOption();
        var configOption = ConfigOption();
        var command = new Command("check", "Verifica URLs cadastradas para um ambiente.");
        command.Add(projectOption);
        command.Add(envOption);
        command.Add(configOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var environment = await LoadEnvironmentAsync(services, parseResult.GetRequiredValue(projectOption), parseResult.GetRequiredValue(envOption), parseResult.GetValue(configOption), cancellationToken);
            if (environment is null)
            {
                return 1;
            }

            var healthCheck = services.GetRequiredService<IUrlHealthCheckService>();
            var allHealthy = true;
            foreach (var url in environment.Urls)
            {
                var result = await healthCheck.CheckAsync(url, cancellationToken);
                allHealthy &= result.Success;
                CommandHelpers.PrintResult(result.Success, result.Message);
            }

            return allHealthy ? 0 : 1;
        });
        return command;
    }

    private static async Task<Core.Models.EnvironmentConfiguration?> LoadEnvironmentAsync(
        IServiceProvider services,
        string projectName,
        string environmentName,
        string? configPath,
        CancellationToken cancellationToken)
    {
        var (configuration, exitCode) = await CommandHelpers.LoadConfigurationAsync(services, configPath, cancellationToken);
        if (configuration is null || exitCode != 0)
        {
            return null;
        }

        var project = CommandHelpers.GetProject(configuration, projectName);
        return project is null ? null : CommandHelpers.GetEnvironment(project, environmentName);
    }

    private static Option<string> ProjectOption() => new("--project", "-p")
    {
        Required = true,
        Description = "Nome do projeto."
    };

    private static Option<string> EnvOption() => new("--env", "-e")
    {
        Required = true,
        Description = "Nome do ambiente."
    };

    private static Option<string?> ConfigOption() => new("--config", "-c")
    {
        Description = "Caminho do arquivo opscli.yml."
    };
}
