namespace OpsCli.Core.Models;

public sealed class UrlConfiguration
{
    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public List<int> ExpectedStatusCodes { get; set; } = [];
}
