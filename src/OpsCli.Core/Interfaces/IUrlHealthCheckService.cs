using OpsCli.Core.Models;

namespace OpsCli.Core.Interfaces;

public interface IUrlHealthCheckService
{
    Task<UrlHealthCheckResult> CheckAsync(UrlConfiguration url, CancellationToken cancellationToken = default);
}
