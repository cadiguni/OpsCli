using OpsCli.Core.Interfaces;
using OpsCli.Core.Models;
using OpsCli.Core.Services;

namespace OpsCli.Tests.ProjectCheck;

public sealed class ProjectCheckServiceTests
{
    [Fact]
    public async Task CheckAsync_AggregatesRepositoryYamlAndUrlResults()
    {
        var project = CreateProject("C:\\Repos\\api-finops", "pipelines/deploy-dev.yml");
        var service = new ProjectCheckService(
            new StubRepositoryService(),
            new StubYamlValidationService(true),
            new StubUrlHealthCheckService(true));

        var result = await service.CheckAsync(project, "dev");

        Assert.True(result.Success);
        Assert.Equal(4, result.TotalChecks);
        Assert.Equal(4, result.Successes);
        Assert.Equal(0, result.Failures);
    }

    [Fact]
    public async Task CheckAsync_ResolvesRelativeYamlFromRepositoryPath()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "opscli-tests", Guid.NewGuid().ToString("N"), "sample-api");
        var yamlService = new CapturingYamlValidationService(true);
        var project = CreateProject(repositoryPath, "pipelines/deploy-dev.yml");
        var service = new ProjectCheckService(
            new StubRepositoryService(),
            yamlService,
            new StubUrlHealthCheckService(true));

        await service.CheckAsync(project, "dev");

        Assert.Equal(
            Path.GetFullPath(Path.Combine(repositoryPath, "pipelines/deploy-dev.yml")),
            Path.GetFullPath(yamlService.Paths.Single()));
    }

    [Fact]
    public async Task CheckAsync_ReturnsFailureWhenYamlOrUrlFails()
    {
        var project = CreateProject("C:\\Repos\\sample-api", "pipelines/deploy-dev.yml");
        var service = new ProjectCheckService(
            new StubRepositoryService(),
            new StubYamlValidationService(false),
            new StubUrlHealthCheckService(false));

        var result = await service.CheckAsync(project, "dev");

        Assert.False(result.Success);
        Assert.Equal(2, result.Failures);
    }

    [Fact]
    public async Task CheckAsync_ReturnsSuccessWhenAllChecksPass()
    {
        var project = CreateProject("C:\\Repos\\sample-api", "pipelines/deploy-dev.yml");
        var service = new ProjectCheckService(
            new StubRepositoryService(),
            new StubYamlValidationService(true),
            new StubUrlHealthCheckService(true));

        var result = await service.CheckAsync(project, "dev");

        Assert.True(result.Success);
        Assert.Equal(0, result.Failures);
    }

    private static ProjectConfiguration CreateProject(string repositoryPath, string yamlFile)
    {
        return new ProjectConfiguration
        {
            Repositories =
            [
                new RepositoryConfiguration
                {
                    Name = "sample-api",
                    Path = repositoryPath,
                    DefaultBranch = "main"
                }
            ],
            Environments = new Dictionary<string, EnvironmentConfiguration>(StringComparer.OrdinalIgnoreCase)
            {
                ["dev"] = new EnvironmentConfiguration
                {
                    YamlFiles = [yamlFile],
                    Urls =
                    [
                        new UrlConfiguration
                        {
                            Name = "Health API",
                            Url = "https://example.test/health",
                            ExpectedStatusCodes = [200]
                        }
                    ]
                }
            }
        };
    }

    private sealed class StubRepositoryService : IRepositoryService
    {
        public Task<IReadOnlyList<RepositoryStatus>> GetStatusesAsync(IEnumerable<RepositoryConfiguration> repositories, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<RepositoryStatus> statuses = repositories
                .Select(repository => new RepositoryStatus(repository.Name, repository.Path, true, true, "main", false, "Arvore de trabalho limpa."))
                .ToList();

            return Task.FromResult(statuses);
        }
    }

    private sealed class StubYamlValidationService : IYamlValidationService
    {
        private readonly bool _success;

        public StubYamlValidationService(bool success)
        {
            _success = success;
        }

        public Task<YamlValidationResult> ValidateAsync(string path, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new YamlValidationResult(path, true, _success, _success ? "YAML valido." : "YAML invalido."));
        }
    }

    private sealed class CapturingYamlValidationService : IYamlValidationService
    {
        private readonly bool _success;

        public CapturingYamlValidationService(bool success)
        {
            _success = success;
        }

        public List<string> Paths { get; } = [];

        public Task<YamlValidationResult> ValidateAsync(string path, CancellationToken cancellationToken = default)
        {
            Paths.Add(path);
            return Task.FromResult(new YamlValidationResult(path, true, _success, _success ? "YAML valido." : "YAML invalido."));
        }
    }

    private sealed class StubUrlHealthCheckService : IUrlHealthCheckService
    {
        private readonly bool _success;

        public StubUrlHealthCheckService(bool success)
        {
            _success = success;
        }

        public Task<UrlHealthCheckResult> CheckAsync(UrlConfiguration url, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var statusCode = _success ? 200 : 500;
            return Task.FromResult(new UrlHealthCheckResult(url.Name, url.Url, _success, statusCode, 84, false, "OK"));
        }
    }
}
