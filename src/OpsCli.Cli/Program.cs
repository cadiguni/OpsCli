using System.CommandLine;
using OpsCli.Cli;
using OpsCli.Cli.Commands;

var services = DependencyInjection.BuildServiceProvider();

var rootCommand = new RootCommand("OpsCli - automacao local para rotinas de DevOps.");
rootCommand.Add(ConfigCommands.Create(services));
rootCommand.Add(ProjectCommands.Create(services));
rootCommand.Add(RepoCommands.Create(services));
rootCommand.Add(YamlCommands.Create(services));
rootCommand.Add(UrlCommands.Create(services));

return await rootCommand.Parse(args).InvokeAsync(new InvocationConfiguration(), CancellationToken.None);
