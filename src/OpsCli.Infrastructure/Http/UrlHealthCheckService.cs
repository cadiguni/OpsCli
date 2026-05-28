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

    public async Task<UrlHealthCheckResult> CheckAsync(UrlConfiguration url, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url.Url);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var statusCode = (int)response.StatusCode;
            var expectedStatusCodes = url.ExpectedStatusCodes.Count > 0 ? url.ExpectedStatusCodes : [200];
            var success = expectedStatusCodes.Contains(statusCode);
            var message = success
                ? $"{url.Name}: HTTP {statusCode} em {url.Url}."
                : $"{url.Name}: HTTP {statusCode} em {url.Url}. Esperado: {string.Join(", ", expectedStatusCodes)}.";

            return new UrlHealthCheckResult(url.Name, url.Url, success, statusCode, message);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or UriFormatException or InvalidOperationException)
        {
            return new UrlHealthCheckResult(url.Name, url.Url, false, null, $"{url.Name}: falha ao consultar {url.Url}. {ex.Message}");
        }
    }
}
