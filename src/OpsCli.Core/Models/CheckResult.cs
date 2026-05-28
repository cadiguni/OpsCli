namespace OpsCli.Core.Models;

public sealed class CheckResult
{
    public CheckResult(string name, bool success, string message)
    {
        Name = name;
        Success = success;
        Message = message;
    }

    public string Name { get; }

    public bool Success { get; }

    public string Message { get; }
}
