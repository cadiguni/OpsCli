namespace OpsCli.Core.Models;

public sealed class ProjectCheckResult
{
    public ProjectCheckResult(
        IReadOnlyList<RepositoryStatus> repositories,
        IReadOnlyList<YamlValidationResult> yamlFiles,
        IReadOnlyList<UrlHealthCheckResult> urls)
    {
        Repositories = repositories;
        YamlFiles = yamlFiles;
        Urls = urls;
    }

    public IReadOnlyList<RepositoryStatus> Repositories { get; }

    public IReadOnlyList<YamlValidationResult> YamlFiles { get; }

    public IReadOnlyList<UrlHealthCheckResult> Urls { get; }

    public int TotalChecks => Repositories.Count * 2 + YamlFiles.Count + Urls.Count;

    public int Failures =>
        Repositories.Sum(repository => RepositoryFailures(repository)) +
        YamlFiles.Count(yaml => !yaml.Exists || !yaml.IsValid) +
        Urls.Count(url => !url.Success);

    public int Successes => TotalChecks - Failures;

    public bool Success => Failures == 0;

    private static int RepositoryFailures(RepositoryStatus repository)
    {
        var failures = 0;
        if (!repository.Exists || !repository.IsGitRepository)
        {
            failures++;
        }

        if (repository.HasChanges)
        {
            failures++;
        }

        return failures;
    }
}
