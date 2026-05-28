using System.Diagnostics;
using OpsCli.Core.Interfaces;
using OpsCli.Core.Models;

namespace OpsCli.Infrastructure.Http;

public sealed class UrlHealthCheckService : IUrlHealthCheckService
{
    private readonly HttpClient _httpClient;

    public UrlHealthCheckService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UrlHealthCheckResult> CheckAsync(UrlConfiguration url, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout ?? TimeSpan.FromSeconds(5));

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url.Url);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutSource.Token);
            stopwatch.Stop();
            var statusCode = (int)response.StatusCode;
            var expectedStatusCodes = url.ExpectedStatusCodes.Count > 0 ? url.ExpectedStatusCodes : [200];
            var success = expectedStatusCodes.Contains(statusCode);
            var message = success
                ? $"{url.Name} respondeu {statusCode} em {stopwatch.ElapsedMilliseconds} ms"
                : $"{url.Name} respondeu {statusCode} em {stopwatch.ElapsedMilliseconds} ms. Esperado: {string.Join(", ", expectedStatusCodes)}.";

            return new UrlHealthCheckResult(url.Name, url.Url, success, statusCode, stopwatch.ElapsedMilliseconds, false, message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return new UrlHealthCheckResult(url.Name, url.Url, false, null, stopwatch.ElapsedMilliseconds, true, $"{url.Name}: timeout ao consultar {url.Url}. {ex.Message}");
        }
        catch (Exception ex) when (ex is HttpRequestException or UriFormatException or InvalidOperationException)
        {
            stopwatch.Stop();
            return new UrlHealthCheckResult(url.Name, url.Url, false, null, stopwatch.ElapsedMilliseconds, false, $"{url.Name}: falha ao consultar {url.Url}. {ex.Message}");
        }
    }
}
