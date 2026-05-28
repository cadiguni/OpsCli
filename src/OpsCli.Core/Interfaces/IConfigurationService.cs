using OpsCli.Core.Models;

namespace OpsCli.Core.Interfaces;

public interface IConfigurationService
{
    string ResolveConfigurationPath(string? path);

    bool ConfigurationExists(string? path = null);

    Task CreateSampleConfigurationAsync(string? path = null, bool overwrite = false, CancellationToken cancellationToken = default);

    Task<OpsCliConfiguration> LoadAsync(string? path = null, CancellationToken cancellationToken = default);
}
