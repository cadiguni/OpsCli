using OpsCli.Core.Models;

namespace OpsCli.Core.Services;

public sealed class ProjectConfigurationResolver
{
    public LookupResult<ProjectConfiguration> GetProject(OpsCliConfiguration configuration, string projectName)
    {
        if (configuration.Projects.TryGetValue(projectName, out var project))
        {
            return LookupResult<ProjectConfiguration>.Found(project);
        }

        return LookupResult<ProjectConfiguration>.NotFound($"Projeto nao encontrado: {projectName}");
    }

    public LookupResult<EnvironmentConfiguration> GetEnvironment(ProjectConfiguration project, string environmentName)
    {
        if (project.Environments.TryGetValue(environmentName, out var environment))
        {
            return LookupResult<EnvironmentConfiguration>.Found(environment);
        }

        return LookupResult<EnvironmentConfiguration>.NotFound($"Ambiente nao encontrado: {environmentName}");
    }
}
