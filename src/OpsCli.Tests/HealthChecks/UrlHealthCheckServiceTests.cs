using System.Net;
using OpsCli.Core.Models;
using OpsCli.Infrastructure.Http;

namespace OpsCli.Tests.HealthChecks;

public sealed class UrlHealthCheckServiceTests
{
    [Fact]
    public async Task CheckAsync_ReturnsSuccessWhenStatusCodeIsExpected()
    {
        var service = new UrlHealthCheckService(new HttpClient(new StaticResponseHandler(HttpStatusCode.OK)));
        var url = new UrlConfiguration
        {
            Name = "Health",
            Url = "https://example.test/health",
            ExpectedStatusCodes = [200]
        };

        var result = await service.CheckAsync(url);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task CheckAsync_ReturnsFailureWhenStatusCodeIsUnexpected()
    {
        var service = new UrlHealthCheckService(new HttpClient(new StaticResponseHandler(HttpStatusCode.InternalServerError)));
        var url = new UrlConfiguration
        {
            Name = "Health",
            Url = "https://example.test/health",
            ExpectedStatusCodes = [200]
        };

        var result = await service.CheckAsync(url);

        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
    }

    private sealed class StaticResponseHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public StaticResponseHandler(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode));
        }
    }
}
