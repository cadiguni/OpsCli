namespace OpsCli.Infrastructure.Configuration;

internal static class SampleConfiguration
{
    public const string Content =
        """
        workspace: "C:\\Projetos\\Repos"

        projects:
          sample-api:
            description: "API de exemplo"
            repositories:
              - name: "sample-api"
                path: "C:\\Projetos\\Repos\\sample-api"
                defaultBranch: "main"

            environments:
              dev:
                yamlFiles:
                  - "pipelines/deploy-dev.yml"
                urls:
                  - name: "Health API"
                    url: "https://api-dev.exemplo.com/health"
                    expectedStatusCodes:
                      - 200
        """;
}
