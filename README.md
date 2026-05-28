# OpsCli

OpsCli e uma ferramenta de linha de comando em C#/.NET 10 para automatizar validacoes locais comuns em rotinas de DevOps.

## Objetivo

Centralizar consultas e validacoes repetitivas em comandos simples no terminal, reduzindo verificacoes manuais sobre projetos, repositorios Git, pipelines YAML e URLs de ambientes.

Nesta fase, a ferramenta nao integra com Azure DevOps, Azure ou autenticacao. O MVP e local, executavel e preparado para evoluir.

## Problema Que Resolve

Em rotinas de DevOps, e comum alternar entre arquivos YAML, diretorios de repositorios, status Git e URLs de ambientes. OpsCli consolida essas verificacoes em uma configuracao local e fornece comandos padronizados que podem ser usados manualmente ou futuramente em CI/CD.

## Stack

- C# e .NET 10
- Console Application
- System.CommandLine
- YamlDotNet
- HttpClient
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging
- xUnit

## Arquitetura

- `src/OpsCli.Cli`: comandos, argumentos e output de terminal.
- `src/OpsCli.Core`: modelos, interfaces, resolucao de projeto/ambiente e agregacao do `project check`.
- `src/OpsCli.Infrastructure`: leitura YAML, validacao YAML, Git local e HTTP.
- `src/OpsCli.Tests`: testes unitarios.
- `samples/opscli.example.yml`: configuracao de exemplo.

## Como Executar Em Desenvolvimento

```powershell
dotnet build
dotnet test

dotnet run --project src/OpsCli.Cli -- config init
dotnet run --project src/OpsCli.Cli -- project list
dotnet run --project src/OpsCli.Cli -- project show sample-api --env dev
dotnet run --project src/OpsCli.Cli -- repos status --project sample-api
dotnet run --project src/OpsCli.Cli -- yaml validate-all --project sample-api --env dev
dotnet run --project src/OpsCli.Cli -- urls check --project sample-api --env dev
dotnet run --project src/OpsCli.Cli -- project check sample-api --env dev
```

Comandos que leem configuracao aceitam `--config <path>`. Sem esse parametro, a CLI procura `opscli.yml` no diretorio atual.

## Configuracao Local

`opscli.yml` e a configuracao local da maquina. Ele pode conter caminhos de repositorios, nomes de projetos e URLs internas, portanto nao deve ser commitado. O arquivo esta no `.gitignore`.

Use `dotnet run --project src/OpsCli.Cli -- config init` para gerar um `opscli.yml` no diretorio atual, ou copie `samples/opscli.example.yml`. Depois substitua os projetos, caminhos e URLs pelos dados do seu ambiente.

`samples/opscli.example.yml` e apenas um modelo generico versionado. Ele nao deve conter dados reais de clientes, URLs internas, subscriptions ou caminhos corporativos.

Exemplo generico:

```yaml
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
```

## Roadmap

- [x] Configuracao local via YAML
- [x] Cadastro e consulta de projetos
- [x] Validacao local de YAML
- [x] Verificacao de URLs
- [x] Status de repositorios Git
- [x] Project Check consolidado
- [ ] Integracao read-only com Azure DevOps Pipelines
- [ ] Consulta de Variable Groups / Libraries
- [ ] Diagnostico de execucao com falha
- [ ] Git clone e git pull seguro
- [ ] Consulta de recursos Azure
- [ ] Deploy preflight
- [ ] Relatorios Markdown e HTML
