using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using OpsCli.Core.Interfaces;
using OpsCli.Core.Models;

namespace OpsCli.Cli.Commands;

public static class RepoCommands
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("repos", "Consulta repositorios locais configurados.");
        command.Add(CreateStatusCommand(services));
        command.Add(CreateExistsCommand(services));
        return command;
    }

    private static Command CreateStatusCommand(IServiceProvider services)
    {
        var configOption = ConfigOption();
        var command = new Command("status", "Consulta status Git dos repositorios configurados.");
        command.Add(configOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var repositories = await LoadRepositoriesAsync(services, parseResult.GetValue(configOption), cancellationToken);
            if (repositories is null)
            {
                return 1;
            }

            var repositoryService = services.GetRequiredService<IRepositoryService>();
            var statuses = await repositoryService.GetStatusesAsync(repositories, cancellationToken);
            foreach (var status in statuses)
            {
                var success = status.Exists && status.IsGitRepository;
                CommandHelpers.PrintResult(success, $"{status.Name}: {status.Path}");
                if (success)
                {
                    Console.WriteLine($"     Branch: {status.Branch}");
                    Console.WriteLine($"     Alteracoes: {(status.HasChanges ? "sim" : "nao")}");
                }
                else
                {
                    Console.WriteLine($"     {status.Details}");
                }
            }

            return statuses.All(status => status.Exists && status.IsGitRepository) ? 0 : 1;
        });
        return command;
    }

    private static Command CreateExistsCommand(IServiceProvider services)
    {
        var configOption = ConfigOption();
        var command = new Command("exists", "Verifica se os diretorios dos repositorios existem.");
        command.Add(configOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var repositories = await LoadRepositoriesAsync(services, parseResult.GetValue(configOption), cancellationToken);
            if (repositories is null)
            {
                return 1;
            }

            var allExist = true;
            foreach (var repository in repositories)
            {
                var exists = Directory.Exists(repository.Path);
                allExist &= exists;
                CommandHelpers.PrintResult(exists, $"{repository.Name}: {repository.Path}");
            }

            return allExist ? 0 : 1;
        });
        return command;
    }

    private static async Task<IReadOnlyList<RepositoryConfiguration>?> LoadRepositoriesAsync(IServiceProvider services, string? configPath, CancellationToken cancellationToken)
    {
        var (configuration, exitCode) = await CommandHelpers.LoadConfigurationAsync(services, configPath, cancellationToken);
        if (configuration is null || exitCode != 0)
        {
            return null;
        }

        return configuration.Projects.Values.SelectMany(project => project.Repositories).ToList();
    }

    private static Option<string?> ConfigOption() => new("--config", "-c")
    {
        Description = "Caminho do arquivo opscli.yml."
    };
}
