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
            Console.WriteLine($"Descricao: {project.Description}");
            Console.WriteLine();
            Console.WriteLine("Repositorios:");
            foreach (var repository in project.Repositories)
            {
                Console.WriteLine($"- {repository.Name} | {repository.Path} | branch padrao: {repository.DefaultBranch}");
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
                Console.WriteLine("YAML:");
                foreach (var yamlFile in environment.YamlFiles)
                {
                    Console.WriteLine($"- {yamlFile}");
                }

                Console.WriteLine("URLs:");
                foreach (var url in environment.Urls)
                {
                    Console.WriteLine($"- {url.Name}: {url.Url} (esperado: {string.Join(", ", url.ExpectedStatusCodes)})");
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
        var configOption = ConfigOption();
        var command = new Command("check", "Executa verificacoes principais de repositorios, YAML e URLs.");
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
            var environmentName = parseResult.GetRequiredValue(envOption);
            var project = CommandHelpers.GetProject(configuration, projectName);
            if (project is null)
            {
                return 1;
            }

            var service = services.GetRequiredService<ProjectCheckService>();
            var results = await service.CheckAsync(project, environmentName, cancellationToken);
            foreach (var result in results)
            {
                CommandHelpers.PrintResult(result.Success, $"{result.Name}: {result.Message}");
            }

            return results.All(result => result.Success) ? 0 : 1;
        });
        return command;
    }

    private static Option<string?> ConfigOption() => new("--config", "-c")
    {
        Description = "Caminho do arquivo opscli.yml."
    };
}
