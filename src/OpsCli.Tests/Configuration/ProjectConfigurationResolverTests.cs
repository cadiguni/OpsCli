using OpsCli.Core.Models;
using OpsCli.Core.Services;

namespace OpsCli.Tests.Configuration;

public sealed class ProjectConfigurationResolverTests
{
    [Fact]
    public void GetProject_ReturnsNotFoundWhenProjectDoesNotExist()
    {
        var configuration = new OpsCliConfiguration();
        var resolver = new ProjectConfigurationResolver();

        var result = resolver.GetProject(configuration, "api-finops");

        Assert.False(result.Success);
        Assert.Contains("Projeto nao encontrado", result.Message);
    }

    [Fact]
    public void GetEnvironment_ReturnsNotFoundWhenEnvironmentDoesNotExist()
    {
        var project = new ProjectConfiguration();
        var resolver = new ProjectConfigurationResolver();

        var result = resolver.GetEnvironment(project, "dev");

        Assert.False(result.Success);
        Assert.Contains("Ambiente nao encontrado", result.Message);
    }
}
