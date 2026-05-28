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
        var projectOption = ProjectOption();
        var command = new Command("status", "Consulta status Git dos repositorios configurados.");
        command.Add(configOption);
        command.Add(projectOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var repositories = await LoadRepositoriesAsync(services, parseResult.GetValue(configOption), parseResult.GetValue(projectOption), cancellationToken);
            if (repositories is null)
            {
                return 1;
            }

            var repositoryService = services.GetRequiredService<IRepositoryService>();
            var statuses = await repositoryService.GetStatusesAsync(repositories, cancellationToken);
            foreach (var status in statuses)
            {
                Console.WriteLine($"Repositório: {status.Name}");
                Console.WriteLine($"Caminho: {status.Path}");
                Console.WriteLine();

                CommandHelpers.PrintResult(status.Exists, "Diretório encontrado");
                if (status.Exists)
                {
                    CommandHelpers.PrintResult(status.IsGitRepository, "Repositório Git encontrado");
                }

                if (status.Exists && status.IsGitRepository)
                {
                    Console.WriteLine($"Branch atual: {status.Branch}");
                    if (status.HasChanges)
                    {
                        Console.WriteLine("⚠ Existem alterações locais não commitadas:");
                        foreach (var line in status.Details.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                        {
                            Console.WriteLine($"  {line}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Status: limpo");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(status.Details))
                {
                    Console.WriteLine(status.Details);
                }

                Console.WriteLine();
            }

            return statuses.All(status => status.Exists && status.IsGitRepository) ? 0 : 1;
        });
        return command;
    }

    private static Command CreateExistsCommand(IServiceProvider services)
    {
        var configOption = ConfigOption();
        var projectOption = ProjectOption();
        var command = new Command("exists", "Verifica se os diretorios dos repositorios existem.");
        command.Add(configOption);
        command.Add(projectOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var repositories = await LoadRepositoriesAsync(services, parseResult.GetValue(configOption), parseResult.GetValue(projectOption), cancellationToken);
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

    private static async Task<IReadOnlyList<RepositoryConfiguration>?> LoadRepositoriesAsync(IServiceProvider services, string? configPath, string? projectName, CancellationToken cancellationToken)
    {
        var (configuration, exitCode) = await CommandHelpers.LoadConfigurationAsync(services, configPath, cancellationToken);
        if (configuration is null || exitCode != 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(projectName))
        {
            var project = CommandHelpers.GetProject(configuration, projectName);
            return project?.Repositories;
        }

        return configuration.Projects.Values.SelectMany(project => project.Repositories).ToList();
    }

    private static Option<string?> ConfigOption() => new("--config", "-c")
    {
        Description = "Caminho do arquivo opscli.yml."
    };

    private static Option<string?> ProjectOption() => new("--project", "-p")
    {
        Description = "Nome do projeto."
    };
}
