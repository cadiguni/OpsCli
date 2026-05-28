using OpsCli.Core.Interfaces;
using OpsCli.Core.Models;

namespace OpsCli.Core.Services;

public sealed class ProjectCheckService
{
    private readonly IRepositoryService _repositoryService;
    private readonly IYamlValidationService _yamlValidationService;
    private readonly IUrlHealthCheckService _urlHealthCheckService;

    public ProjectCheckService(
        IRepositoryService repositoryService,
        IYamlValidationService yamlValidationService,
        IUrlHealthCheckService urlHealthCheckService)
    {
        _repositoryService = repositoryService;
        _yamlValidationService = yamlValidationService;
        _urlHealthCheckService = urlHealthCheckService;
    }

    public async Task<ProjectCheckResult> CheckAsync(ProjectConfiguration project, string environmentName, TimeSpan? urlTimeout = null, CancellationToken cancellationToken = default)
    {
        if (!project.Environments.TryGetValue(environmentName, out var environment))
        {
            throw new InvalidOperationException($"Ambiente nao encontrado: {environmentName}");
        }

        var repositoryStatuses = await _repositoryService.GetStatusesAsync(project.Repositories, cancellationToken);
        var yamlResults = new List<YamlValidationResult>();
        var urlResults = new List<UrlHealthCheckResult>();

        foreach (var yamlFile in environment.YamlFiles)
        {
            var validation = await _yamlValidationService.ValidateAsync(ResolveYamlPath(project, yamlFile), cancellationToken);
            yamlResults.Add(validation);
        }

        foreach (var url in environment.Urls)
        {
            var health = await _urlHealthCheckService.CheckAsync(url, urlTimeout, cancellationToken);
            urlResults.Add(health);
        }

        return new ProjectCheckResult(repositoryStatuses, yamlResults, urlResults);
    }

    private static string ResolveYamlPath(ProjectConfiguration project, string yamlFile)
    {
        if (Path.IsPathRooted(yamlFile) || project.Repositories.Count == 0)
        {
            return yamlFile;
        }

        return Path.Combine(project.Repositories[0].Path, yamlFile);
    }
}
