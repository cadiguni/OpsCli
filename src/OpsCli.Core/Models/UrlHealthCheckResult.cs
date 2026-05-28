namespace OpsCli.Core.Models;

public sealed class UrlHealthCheckResult
{
    public UrlHealthCheckResult(string name, string url, bool success, int? statusCode, long elapsedMilliseconds, bool timedOut, string message)
    {
        Name = name;
        Url = url;
        Success = success;
        StatusCode = statusCode;
        ElapsedMilliseconds = elapsedMilliseconds;
        TimedOut = timedOut;
        Message = message;
    }

    public string Name { get; }

    public string Url { get; }

    public bool Success { get; }

    public int? StatusCode { get; }

    public long ElapsedMilliseconds { get; }

    public bool TimedOut { get; }

    public string Message { get; }
}
