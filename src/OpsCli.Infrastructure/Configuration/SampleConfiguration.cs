namespace OpsCli.Infrastructure.Configuration;

internal static class SampleConfiguration
{
    public const string Content =
        """
        workspace: "C:\\Repos"

        projects:
          api-finops:
            description: "API de analise de custos Azure"
            repositories:
              - name: "api-finops"
                path: "C:\\Repos\\api-finops"
                defaultBranch: "main"

            environments:
              dev:
                yamlFiles:
                  - "pipelines/deploy-dev.yml"
                urls:
                  - name: "Health API"
                    url: "https://localhost:7071/api/health"
                    expectedStatusCodes:
                      - 200
                  - name: "Swagger"
                    url: "https://localhost:7071/swagger/index.html"
                    expectedStatusCodes:
                      - 200

          portal-cliente:
            description: "Frontend do portal"
            repositories:
              - name: "portal-cliente-front"
                path: "C:\\Repos\\portal-cliente-front"
                defaultBranch: "main"

            environments:
              dev:
                yamlFiles:
                  - "pipelines/deploy-dev.yml"
                urls:
                  - name: "Frontend"
                    url: "https://portal-dev.exemplo.com"
                    expectedStatusCodes:
                      - 200
        """;
}
