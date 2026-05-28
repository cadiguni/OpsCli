namespace OpsCli.Core.Models;

public sealed class RepositoryStatus
{
    public RepositoryStatus(string name, string path, bool exists, bool isGitRepository, string branch, bool hasChanges, string details)
    {
        Name = name;
        Path = path;
        Exists = exists;
        IsGitRepository = isGitRepository;
        Branch = branch;
        HasChanges = hasChanges;
        Details = details;
    }

    public string Name { get; }

    public string Path { get; }

    public bool Exists { get; }

    public bool IsGitRepository { get; }

    public string Branch { get; }

    public bool HasChanges { get; }

    public string Details { get; }
}
