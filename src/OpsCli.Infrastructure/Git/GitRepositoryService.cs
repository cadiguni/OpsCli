using System.Diagnostics;
using OpsCli.Core.Interfaces;
using OpsCli.Core.Models;

namespace OpsCli.Infrastructure.Git;

public sealed class GitRepositoryService : IRepositoryService
{
    public async Task<IReadOnlyList<RepositoryStatus>> GetStatusesAsync(IEnumerable<RepositoryConfiguration> repositories, CancellationToken cancellationToken = default)
    {
        var results = new List<RepositoryStatus>();

        foreach (var repository in repositories)
        {
            if (!Directory.Exists(repository.Path))
            {
                results.Add(new RepositoryStatus(repository.Name, repository.Path, false, false, string.Empty, false, "Diretorio nao encontrado."));
                continue;
            }

            var gitDirectory = Path.Combine(repository.Path, ".git");
            if (!Directory.Exists(gitDirectory) && !File.Exists(gitDirectory))
            {
                results.Add(new RepositoryStatus(repository.Name, repository.Path, true, false, string.Empty, false, "Diretorio existe, mas nao parece ser um repositorio Git."));
                continue;
            }

            var branchResult = await RunGitAsync(repository.Path, "branch --show-current", cancellationToken);
            var statusResult = await RunGitAsync(repository.Path, "status --short", cancellationToken);
            if (branchResult.ExitCode != 0 || statusResult.ExitCode != 0)
            {
                var gitErrorDetails = string.Join(
                    Environment.NewLine,
                    new[] { branchResult.Error.Trim(), statusResult.Error.Trim() }.Where(error => !string.IsNullOrWhiteSpace(error)));
                results.Add(new RepositoryStatus(repository.Name, repository.Path, true, false, string.Empty, false, gitErrorDetails));
                continue;
            }

            var branch = branchResult.Output.Trim();
            var lines = statusResult.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var hasChanges = lines.Any();
            var details = hasChanges ? string.Join(Environment.NewLine, lines) : "Arvore de trabalho limpa.";
            results.Add(new RepositoryStatus(repository.Name, repository.Path, true, true, branch, hasChanges, details));
        }

        return results;
    }

    private static async Task<(int ExitCode, string Output, string Error)> RunGitAsync(string workingDirectory, string arguments, CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo("git", $"-C \"{workingDirectory}\" {arguments}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return (process.ExitCode, await outputTask, await errorTask);
    }
}
