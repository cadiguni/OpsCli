namespace OpsCli.Core.Models;

public sealed class LookupResult<T>
{
    private LookupResult(T? value, bool success, string message)
    {
        Value = value;
        Success = success;
        Message = message;
    }

    public T? Value { get; }

    public bool Success { get; }

    public string Message { get; }

    public static LookupResult<T> Found(T value) => new(value, true, string.Empty);

    public static LookupResult<T> NotFound(string message) => new(default, false, message);
}
