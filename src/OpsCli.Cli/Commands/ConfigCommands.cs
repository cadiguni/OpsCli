using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using OpsCli.Core.Interfaces;

namespace OpsCli.Cli.Commands;

public static class ConfigCommands
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("config", "Gerencia a configuracao local.");
        command.Add(CreateInitCommand(services));
        return command;
    }

    private static Command CreateInitCommand(IServiceProvider services)
    {
        var configOption = new Option<string?>("--config", "-c")
        {
            Description = "Caminho do arquivo opscli.yml."
        };
        var forceOption = new Option<bool>("--force", "-f")
        {
            Description = "Sobrescreve o arquivo existente."
        };

        var command = new Command("init", "Cria um arquivo opscli.yml de exemplo no diretorio atual.");
        command.Add(configOption);
        command.Add(forceOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var configPath = parseResult.GetValue(configOption);
            var force = parseResult.GetValue(forceOption);
            var configurationService = services.GetRequiredService<IConfigurationService>();
            var resolvedPath = configurationService.ResolveConfigurationPath(configPath);

            try
            {
                var overwrite = force;
                if (!overwrite && configurationService.ConfigurationExists(configPath))
                {
                    Console.Write($"Arquivo de configuracao ja existe: {resolvedPath}. Sobrescrever? [y/N] ");
                    var answer = Console.ReadLine();
                    overwrite = string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(answer, "yes", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(answer, "s", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(answer, "sim", StringComparison.OrdinalIgnoreCase);

                    if (!overwrite)
                    {
                        Console.WriteLine("Operacao cancelada.");
                        return 1;
                    }
                }

                await configurationService.CreateSampleConfigurationAsync(configPath, overwrite, cancellationToken);
                Console.WriteLine($"[OK] Arquivo de configuracao criado: {resolvedPath}");
                Console.WriteLine("Edite o arquivo para cadastrar seus projetos e ambientes.");
                return 0;
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Use --force para sobrescrever.");
                return 1;
            }
        });

        return command;
    }
}
