using OpsCli.Core.Models;

namespace OpsCli.Core.Interfaces;

public interface IRepositoryService
{
    Task<IReadOnlyList<RepositoryStatus>> GetStatusesAsync(IEnumerable<RepositoryConfiguration> repositories, CancellationToken cancellationToken = default);
}
