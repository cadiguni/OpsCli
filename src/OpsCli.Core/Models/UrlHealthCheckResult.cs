namespace OpsCli.Core.Models;

public sealed class UrlHealthCheckResult
{
    public UrlHealthCheckResult(string name, string url, bool success, int? statusCode, string message)
    {
        Name = name;
        Url = url;
        Success = success;
        StatusCode = statusCode;
        Message = message;
    }

    public string Name { get; }

    public string Url { get; }

    public bool Success { get; }

    public int? StatusCode { get; }

    public string Message { get; }
}
