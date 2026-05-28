namespace OpsCli.Core.Models;

public sealed class YamlValidationResult
{
    public YamlValidationResult(string path, bool exists, bool isValid, string message)
    {
        Path = path;
        Exists = exists;
        IsValid = isValid;
        Message = message;
    }

    public string Path { get; }

    public bool Exists { get; }

    public bool IsValid { get; }

    public string Message { get; }
}
