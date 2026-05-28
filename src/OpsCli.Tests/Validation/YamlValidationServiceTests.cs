using OpsCli.Infrastructure.Validation;

namespace OpsCli.Tests.Validation;

public sealed class YamlValidationServiceTests
{
    [Fact]
    public async Task ValidateAsync_ReturnsValidForParseableYaml()
    {
        var file = CreateTempFile("name: opscli");
        var service = new YamlValidationService();

        var result = await service.ValidateAsync(file);

        Assert.True(result.Exists);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsInvalidForMalformedYaml()
    {
        var file = CreateTempFile("name: [opscli");
        var service = new YamlValidationService();

        var result = await service.ValidateAsync(file);

        Assert.True(result.Exists);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFullPathWhenYamlDoesNotExist()
    {
        var directory = Path.Combine(Path.GetTempPath(), "opscli-tests", Guid.NewGuid().ToString("N"));
        var missingFile = Path.Combine(directory, "pipelines", "deploy-dev.yml");
        var service = new YamlValidationService();

        var result = await service.ValidateAsync(missingFile);

        Assert.False(result.Exists);
        Assert.False(result.IsValid);
        Assert.Contains("[ERRO] Arquivo YAML nao encontrado:", result.Message);
        Assert.Contains(Path.GetFullPath(missingFile), result.Message);
    }

    private static string CreateTempFile(string content)
    {
        var directory = Path.Combine(Path.GetTempPath(), "opscli-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, "pipeline.yml");
        File.WriteAllText(file, content);
        return file;
    }
}
