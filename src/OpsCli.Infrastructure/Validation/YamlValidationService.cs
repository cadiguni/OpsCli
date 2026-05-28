using OpsCli.Core.Interfaces;
using OpsCli.Core.Models;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace OpsCli.Infrastructure.Validation;

public sealed class YamlValidationService : IYamlValidationService
{
    public async Task<YamlValidationResult> ValidateAsync(string path, CancellationToken cancellationToken = default)
    {
        var resolvedPath = Path.GetFullPath(path);
        if (!File.Exists(resolvedPath))
        {
            return new YamlValidationResult(resolvedPath, false, false, $"[ERRO] Arquivo YAML nao encontrado:{Environment.NewLine}{resolvedPath}");
        }

        try
        {
            await using var stream = File.OpenRead(resolvedPath);
            using var reader = new StreamReader(stream);
            var yaml = new YamlStream();
            yaml.Load(reader);
            return new YamlValidationResult(resolvedPath, true, true, "YAML valido.");
        }
        catch (YamlException ex)
        {
            return new YamlValidationResult(resolvedPath, true, false, $"YAML invalido: {ex.Message}");
        }
    }
}
