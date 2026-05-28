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

    public async Task<IReadOnlyList<CheckResult>> CheckAsync(ProjectConfiguration project, string environmentName, CancellationToken cancellationToken = default)
    {
        if (!project.Environments.TryGetValue(environmentName, out var environment))
        {
            return [new CheckResult("environment", false, $"Ambiente nao encontrado: {environmentName}")];
        }

        var results = new List<CheckResult>();

        var repositoryStatuses = await _repositoryService.GetStatusesAsync(project.Repositories, cancellationToken);
        foreach (var status in repositoryStatuses)
        {
            var success = status.Exists && status.IsGitRepository;
            var message = success
                ? $"Repositorio encontrado em {status.Path}. Branch: {status.Branch}. Alteracoes: {(status.HasChanges ? "sim" : "nao")}."
                : status.Details;

            results.Add(new CheckResult($"repo:{status.Name}", success, message));
        }

        foreach (var yamlFile in environment.YamlFiles)
        {
            var validation = await _yamlValidationService.ValidateAsync(ResolveYamlPath(project, yamlFile), cancellationToken);
            results.Add(new CheckResult($"yaml:{yamlFile}", validation.Exists && validation.IsValid, validation.Message));
        }

        foreach (var url in environment.Urls)
        {
            var health = await _urlHealthCheckService.CheckAsync(url, cancellationToken);
            results.Add(new CheckResult($"url:{url.Name}", health.Success, health.Message));
        }

        return results;
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
