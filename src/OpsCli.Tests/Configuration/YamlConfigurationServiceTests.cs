using OpsCli.Infrastructure.Configuration;

namespace OpsCli.Tests.Configuration;

public sealed class YamlConfigurationServiceTests
{
    [Fact]
    public async Task LoadAsync_ReadsProjectsAndEnvironments()
    {
        var directory = CreateTempDirectory();
        var configPath = Path.Combine(directory, "opscli.yml");
        await File.WriteAllTextAsync(configPath,
            """
            workspace: "C:\\Repos"
            projects:
              api-finops:
                description: "API"
                repositories:
                  - name: "api-finops"
                    path: "C:\\Repos\\api-finops"
                    defaultBranch: "main"
                environments:
                  dev:
                    yamlFiles:
                      - "pipelines/deploy-dev.yml"
                    urls:
                      - name: "Health"
                        url: "https://localhost/health"
                        expectedStatusCodes:
                          - 200
            """);

        var service = new YamlConfigurationService();
        var configuration = await service.LoadAsync(configPath);

        Assert.Equal("C:\\Repos", configuration.Workspace);
        Assert.True(configuration.Projects.ContainsKey("api-finops"));
        Assert.True(configuration.Projects["API-FINOPS"].Environments.ContainsKey("DEV"));
    }

    [Fact]
    public async Task CreateSampleConfigurationAsync_DoesNotOverwriteByDefault()
    {
        var directory = CreateTempDirectory();
        var configPath = Path.Combine(directory, "opscli.yml");
        await File.WriteAllTextAsync(configPath, "workspace: test");

        var service = new YamlConfigurationService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateSampleConfigurationAsync(configPath));
    }

    [Fact]
    public async Task LoadAsync_ThrowsWhenConfigurationDoesNotExist()
    {
        var service = new YamlConfigurationService();
        var missingPath = Path.Combine(Path.GetTempPath(), "opscli-tests", Guid.NewGuid().ToString("N"), "opscli.yml");

        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => service.LoadAsync(missingPath));
        Assert.Contains("[ERRO] Arquivo de configuracao nao encontrado:", exception.Message);
        Assert.Contains(missingPath, exception.Message);
        Assert.Contains("Execute 'config init' para criar um arquivo de exemplo.", exception.Message);
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "opscli-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
