using OpsCli.Core.Models;

namespace OpsCli.Core.Interfaces;

public interface IYamlValidationService
{
    Task<YamlValidationResult> ValidateAsync(string path, CancellationToken cancellationToken = default);
}
