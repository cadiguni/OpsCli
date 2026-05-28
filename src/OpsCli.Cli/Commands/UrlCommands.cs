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
        var timeoutOption = TimeoutOption();
        var configOption = ConfigOption();
        var command = new Command("check", "Verifica URLs cadastradas para um ambiente.");
        command.Add(projectOption);
        command.Add(envOption);
        command.Add(timeoutOption);
        command.Add(configOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var projectName = parseResult.GetRequiredValue(projectOption);
            var environmentName = parseResult.GetRequiredValue(envOption);
            var environment = await LoadEnvironmentAsync(services, projectName, environmentName, parseResult.GetValue(configOption), cancellationToken);
            if (environment is null)
            {
                return 1;
            }

            Console.WriteLine($"Verificação de URLs - {projectName} / {environmentName}");
            Console.WriteLine();
            Console.WriteLine($"{"URL",-44} {"Status",-10} {"Tempo",-10} Resultado");

            var healthCheck = services.GetRequiredService<IUrlHealthCheckService>();
            var allHealthy = true;
            var timeout = TimeSpan.FromSeconds(parseResult.GetValue(timeoutOption));
            foreach (var url in environment.Urls)
            {
                var result = await healthCheck.CheckAsync(url, timeout, cancellationToken);
                allHealthy &= result.Success;
                var status = result.StatusCode?.ToString() ?? (result.TimedOut ? "Timeout" : "Falha");
                var outcome = result.Success ? "✓ OK" : "✗ Falha";
                Console.WriteLine($"{url.Name,-44} {status,-10} {$"{result.ElapsedMilliseconds} ms",-10} {outcome}");
            }

            return allHealthy ? 0 : 1;
        });
        return command;
    }

    private static async Task<global::OpsCli.Core.Models.EnvironmentConfiguration?> LoadEnvironmentAsync(
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

    private static Option<int> TimeoutOption()
    {
        var option = new Option<int>("--timeout-seconds")
        {
            Description = "Timeout das requisições HTTP em segundos."
        };
        option.DefaultValueFactory = _ => 5;
        return option;
    }
}
