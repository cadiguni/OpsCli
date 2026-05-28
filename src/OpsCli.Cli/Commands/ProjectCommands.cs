using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using OpsCli.Core.Services;

namespace OpsCli.Cli.Commands;

public static class ProjectCommands
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("project", "Consulta e valida projetos configurados.");
        command.Add(CreateListCommand(services));
        command.Add(CreateShowCommand(services));
        command.Add(CreateCheckCommand(services));
        return command;
    }

    private static Command CreateListCommand(IServiceProvider services)
    {
        var configOption = ConfigOption();
        var command = new Command("list", "Lista projetos configurados.");
        command.Add(configOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var (configuration, exitCode) = await CommandHelpers.LoadConfigurationAsync(services, parseResult.GetValue(configOption), cancellationToken);
            if (configuration is null)
            {
                return exitCode;
            }

            Console.WriteLine("Projetos configurados:");
            Console.WriteLine();
            Console.WriteLine($"{"Nome",-24} Descricao");
            foreach (var project in configuration.Projects.OrderBy(p => p.Key))
            {
                Console.WriteLine($"{project.Key,-24} {project.Value.Description}");
            }

            return 0;
        });
        return command;
    }

    private static Command CreateShowCommand(IServiceProvider services)
    {
        var projectArgument = new Argument<string>("name");
        var envOption = new Option<string?>("--env")
        {
            Description = "Ambiente do projeto."
        };
        var configOption = ConfigOption();
        var command = new Command("show", "Exibe detalhes de um projeto e, opcionalmente, de um ambiente.");
        command.Add(projectArgument);
        command.Add(envOption);
        command.Add(configOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var (configuration, exitCode) = await CommandHelpers.LoadConfigurationAsync(services, parseResult.GetValue(configOption), cancellationToken);
            if (configuration is null)
            {
                return exitCode;
            }

            var projectName = parseResult.GetRequiredValue(projectArgument);
            var project = CommandHelpers.GetProject(configuration, projectName);
            if (project is null)
            {
                return 1;
            }

            Console.WriteLine($"Projeto: {projectName}");
            Console.WriteLine($"Descrição: {project.Description}");
            Console.WriteLine();
            Console.WriteLine("Repositórios:");
            foreach (var repository in project.Repositories)
            {
                Console.WriteLine($"- {repository.Name}: {repository.Path}");
            }

            var environmentName = parseResult.GetValue(envOption);
            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                var environment = CommandHelpers.GetEnvironment(project, environmentName);
                if (environment is null)
                {
                    return 1;
                }

                Console.WriteLine();
                Console.WriteLine($"Ambiente: {environmentName}");
                Console.WriteLine();
                Console.WriteLine("Arquivos YAML:");
                foreach (var yamlFile in environment.YamlFiles)
                {
                    Console.WriteLine($"- {yamlFile}");
                }

                Console.WriteLine("URLs:");
                foreach (var url in environment.Urls)
                {
                    Console.WriteLine($"- {url.Name}: {url.Url}");
                }
            }

            return 0;
        });
        return command;
    }

    private static Command CreateCheckCommand(IServiceProvider services)
    {
        var projectArgument = new Argument<string>("name");
        var envOption = new Option<string>("--env") { Required = true, Description = "Ambiente do projeto." };
        var timeoutOption = new Option<int>("--timeout-seconds")
        {
            Description = "Timeout das verificações HTTP em segundos."
        };
        timeoutOption.DefaultValueFactory = _ => 5;
        var configOption = ConfigOption();
        var command = new Command("check", "Executa verificacoes principais de repositorios, YAML e URLs.");
        command.Add(projectArgument);
        command.Add(envOption);
        command.Add(timeoutOption);
        command.Add(configOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var (configuration, exitCode) = await CommandHelpers.LoadConfigurationAsync(services, parseResult.GetValue(configOption), cancellationToken);
            if (configuration is null)
            {
                return exitCode;
            }

            var projectName = parseResult.GetRequiredValue(projectArgument);
            var environmentName = parseResult.GetRequiredValue(envOption);
            var project = CommandHelpers.GetProject(configuration, projectName);
            if (project is null)
            {
                return 1;
            }

            var service = services.GetRequiredService<ProjectCheckService>();
            var timeout = TimeSpan.FromSeconds(parseResult.GetValue(timeoutOption));
            var result = await service.CheckAsync(project, environmentName, timeout, cancellationToken);

            Console.WriteLine($"Project Check - {projectName} / {environmentName}");
            Console.WriteLine();
            Console.WriteLine("Repositórios:");
            foreach (var repository in result.Repositories)
            {
                CommandHelpers.PrintResult(repository.Exists, $"{repository.Name} encontrado");
                CommandHelpers.PrintResult(repository.IsGitRepository, "Repositório Git encontrado");
                if (repository.IsGitRepository)
                {
                    CommandHelpers.PrintResult(true, $"Branch atual: {repository.Branch}");
                    if (repository.HasChanges)
                    {
                        Console.WriteLine("⚠ Existem alterações locais não commitadas:");
                        foreach (var line in repository.Details.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                        {
                            Console.WriteLine($"  {line}");
                        }
                    }
                    else
                    {
                        CommandHelpers.PrintResult(true, "Sem alterações locais");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("YAML:");
            foreach (var yaml in result.YamlFiles)
            {
                var displayPath = DisplayYamlPath(project, yaml.Path);
                CommandHelpers.PrintResult(yaml.Exists && yaml.IsValid, $"{displayPath} {(yaml.Exists && yaml.IsValid ? "válido" : yaml.Message)}");
            }

            Console.WriteLine();
            Console.WriteLine("URLs:");
            foreach (var url in result.Urls)
            {
                var status = url.StatusCode?.ToString() ?? (url.TimedOut ? "Timeout" : "Falha");
                CommandHelpers.PrintResult(url.Success, $"{url.Name} respondeu {status} em {url.ElapsedMilliseconds} ms");
            }

            Console.WriteLine();
            Console.WriteLine("Resumo:");
            Console.WriteLine($"Verificações executadas: {result.TotalChecks}");
            Console.WriteLine($"Sucessos: {result.Successes}");
            Console.WriteLine($"Falhas: {result.Failures}");
            Console.WriteLine();
            Console.WriteLine(result.Success
                ? "Resultado: projeto válido."
                : "Resultado: projeto possui falhas que precisam ser corrigidas.");

            return result.Success ? 0 : 1;
        });
        return command;
    }

    private static Option<string?> ConfigOption() => new("--config", "-c")
    {
        Description = "Caminho do arquivo opscli.yml."
    };

    private static string DisplayYamlPath(global::OpsCli.Core.Models.ProjectConfiguration project, string path)
    {
        var repositoryPath = project.Repositories.FirstOrDefault()?.Path;
        if (string.IsNullOrWhiteSpace(repositoryPath))
        {
            return path;
        }

        var relativePath = Path.GetRelativePath(repositoryPath, path);
        return relativePath.StartsWith("..", StringComparison.Ordinal) ? path : relativePath;
    }
}
