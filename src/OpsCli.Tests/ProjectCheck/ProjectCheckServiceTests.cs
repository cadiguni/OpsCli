using OpsCli.Core.Interfaces;
using OpsCli.Core.Models;
using OpsCli.Core.Services;

namespace OpsCli.Tests.ProjectCheck;

public sealed class ProjectCheckServiceTests
{
    [Fact]
    public async Task CheckAsync_AggregatesRepositoryYamlAndUrlResults()
    {
        var project = new ProjectConfiguration
        {
            Repositories =
            [
                new RepositoryConfiguration
                {
                    Name = "api-finops",
                    Path = "C:\\Repos\\api-finops",
                    DefaultBranch = "main"
                }
            ],
            Environments = new Dictionary<string, EnvironmentConfiguration>(StringComparer.OrdinalIgnoreCase)
            {
                ["dev"] = new EnvironmentConfiguration
                {
                    YamlFiles = ["pipelines/deploy-dev.yml"],
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

        var service = new ProjectCheckService(
            new StubRepositoryService(),
            new StubYamlValidationService(),
            new StubUrlHealthCheckService());

        var result = await service.CheckAsync(project, "dev");

        Assert.True(result.Success);
        Assert.Equal(4, result.TotalChecks);
        Assert.Equal(4, result.Successes);
        Assert.Equal(0, result.Failures);
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
        public Task<YamlValidationResult> ValidateAsync(string path, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new YamlValidationResult(path, true, true, "YAML valido."));
        }
    }

    private sealed class StubUrlHealthCheckService : IUrlHealthCheckService
    {
        public Task<UrlHealthCheckResult> CheckAsync(UrlConfiguration url, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new UrlHealthCheckResult(url.Name, url.Url, true, 200, 84, false, "OK"));
        }
    }
}
