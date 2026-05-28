using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpsCli.Core.Interfaces;
using OpsCli.Core.Services;
using OpsCli.Infrastructure.Configuration;
using OpsCli.Infrastructure.Git;
using OpsCli.Infrastructure.Http;
using OpsCli.Infrastructure.Validation;

namespace OpsCli.Cli;

public static class DependencyInjection
{
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddSingleton<IConfigurationService, YamlConfigurationService>();
        services.AddSingleton<IRepositoryService, GitRepositoryService>();
        services.AddSingleton<IYamlValidationService, YamlValidationService>();
        services.AddHttpClient<IUrlHealthCheckService, UrlHealthCheckService>(client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });
        services.AddSingleton<ProjectCheckService>();
        services.AddSingleton<ProjectConfigurationResolver>();

        return services.BuildServiceProvider();
    }
}
