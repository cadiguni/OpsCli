using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using OpsCli.Core.Interfaces;

namespace OpsCli.Cli.Commands;

public static class YamlCommands
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("yaml", "Valida arquivos YAML locais.");
        command.Add(CreateValidateAllCommand(services));
        command.Add(CreateValidateCommand(services));
        return command;
    }

    private static Command CreateValidateAllCommand(IServiceProvider services)
    {
        var configOption = ConfigOption();
        var projectOption = ProjectOption(required: false);
        var envOption = EnvOption(required: false);
        var command = new Command("validate-all", "Valida todos os YAML cadastrados nos ambientes.");
        command.Add(configOption);
        command.Add(projectOption);
        command.Add(envOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var (configuration, exitCode) = await CommandHelpers.LoadConfigurationAsync(services, parseResult.GetValue(configOption), cancellationToken);
            if (configuration is null)
            {
                return exitCode;
            }

            var projectName = parseResult.GetValue(projectOption);
            var environmentName = parseResult.GetValue(envOption);
            var validator = services.GetRequiredService<IYamlValidationService>();
            var allValid = true;
            var projectEntries = string.IsNullOrWhiteSpace(projectName)
                ? configuration.Projects
                : configuration.Projects.Where(project => string.Equals(project.Key, projectName, StringComparison.OrdinalIgnoreCase)).ToDictionary();

            if (!string.IsNullOrWhiteSpace(projectName) && projectEntries.Count == 0)
            {
                Console.Error.WriteLine($"Projeto nao encontrado: {projectName}");
                return 1;
            }

            foreach (var projectEntry in projectEntries)
            {
                var environmentEntries = string.IsNullOrWhiteSpace(environmentName)
                    ? projectEntry.Value.Environments
                    : projectEntry.Value.Environments.Where(environment => string.Equals(environment.Key, environmentName, StringComparison.OrdinalIgnoreCase)).ToDictionary();

                if (!string.IsNullOrWhiteSpace(environmentName) && environmentEntries.Count == 0)
                {
                    Console.Error.WriteLine($"Ambiente nao encontrado: {environmentName}");
                    return 1;
                }

                foreach (var environmentEntry in environmentEntries)
                {
                    foreach (var yamlFile in environmentEntry.Value.YamlFiles)
                    {
                        var path = CommandHelpers.ResolveYamlPath(projectEntry.Value, yamlFile);
                        var result = await validator.ValidateAsync(path, cancellationToken);
                        allValid &= result.Exists && result.IsValid;
                        if (result.Exists && result.IsValid)
                        {
                            Console.WriteLine($"[OK] YAML valido: {yamlFile}");
                        }
                        else
                        {
                            Console.WriteLine(result.Exists
                                ? $"[FALHA] YAML invalido: {yamlFile}"
                                : result.Message);
                            if (result.Exists)
                            {
                                Console.WriteLine($"Erro: {result.Message}");
                            }
                        }
                    }
                }
            }

            return allValid ? 0 : 1;
        });
        return command;
    }

    private static Command CreateValidateCommand(IServiceProvider services)
    {
        var fileOption = new Option<string>("--file", "-f")
        {
            Required = true,
            Description = "Arquivo YAML a validar."
        };
        var command = new Command("validate", "Valida um arquivo YAML.");
        command.Add(fileOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var validator = services.GetRequiredService<IYamlValidationService>();
            var file = parseResult.GetRequiredValue(fileOption);
            var result = await validator.ValidateAsync(file, cancellationToken);
            if (result.Exists && result.IsValid)
            {
                Console.WriteLine($"[OK] YAML valido: {file}");
            }
            else
            {
                Console.WriteLine(result.Exists
                    ? $"[FALHA] YAML invalido: {file}"
                    : result.Message);
                if (result.Exists)
                {
                    Console.WriteLine($"Erro: {result.Message}");
                }
            }

            return result.Exists && result.IsValid ? 0 : 1;
        });
        return command;
    }

    private static Option<string?> ConfigOption() => new("--config", "-c")
    {
        Description = "Caminho do arquivo opscli.yml."
    };

    private static Option<string?> ProjectOption(bool required) => new("--project", "-p")
    {
        Required = required,
        Description = "Nome do projeto."
    };

    private static Option<string?> EnvOption(bool required) => new("--env", "-e")
    {
        Required = required,
        Description = "Nome do ambiente."
    };
}
