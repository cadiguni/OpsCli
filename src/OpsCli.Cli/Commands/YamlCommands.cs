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
        var command = new Command("validate-all", "Valida todos os YAML cadastrados nos ambientes.");
        command.Add(configOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var (configuration, exitCode) = await CommandHelpers.LoadConfigurationAsync(services, parseResult.GetValue(configOption), cancellationToken);
            if (configuration is null)
            {
                return exitCode;
            }

            var validator = services.GetRequiredService<IYamlValidationService>();
            var allValid = true;
            foreach (var projectEntry in configuration.Projects)
            {
                foreach (var environmentEntry in projectEntry.Value.Environments)
                {
                    foreach (var yamlFile in environmentEntry.Value.YamlFiles)
                    {
                        var path = CommandHelpers.ResolveYamlPath(projectEntry.Value, yamlFile);
                        var result = await validator.ValidateAsync(path, cancellationToken);
                        allValid &= result.Exists && result.IsValid;
                        CommandHelpers.PrintResult(result.Exists && result.IsValid, $"{projectEntry.Key}/{environmentEntry.Key}: {result.Path} - {result.Message}");
                    }
                }
            }

            return allValid ? 0 : 1;
        });
        return command;
    }

    private static Command CreateValidateCommand(IServiceProvider services)
    {
        var pathArgument = new Argument<string>("path");
        var command = new Command("validate", "Valida um arquivo YAML.");
        command.Add(pathArgument);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var validator = services.GetRequiredService<IYamlValidationService>();
            var result = await validator.ValidateAsync(parseResult.GetRequiredValue(pathArgument), cancellationToken);
            CommandHelpers.PrintResult(result.Exists && result.IsValid, $"{result.Path} - {result.Message}");
            return result.Exists && result.IsValid ? 0 : 1;
        });
        return command;
    }

    private static Option<string?> ConfigOption() => new("--config", "-c")
    {
        Description = "Caminho do arquivo opscli.yml."
    };
}
