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

## Comandos Do MVP

```powershell
dotnet run --project src/OpsCli.Cli -- config init
dotnet run --project src/OpsCli.Cli -- project list
dotnet run --project src/OpsCli.Cli -- project show api-finops
dotnet run --project src/OpsCli.Cli -- project show api-finops --env dev
dotnet run --project src/OpsCli.Cli -- repos status --project api-finops
dotnet run --project src/OpsCli.Cli -- yaml validate --file pipelines/deploy-dev.yml
dotnet run --project src/OpsCli.Cli -- yaml validate-all --project api-finops --env dev
dotnet run --project src/OpsCli.Cli -- urls check --project api-finops --env dev
dotnet run --project src/OpsCli.Cli -- project check api-finops --env dev
```

Comandos que leem configuracao aceitam `--config <path>`. Sem esse parametro, a CLI procura `opscli.yml` no diretorio atual.

## Exemplo De opscli.yml

```yaml
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
```

## Como Executar Localmente

```powershell
dotnet restore
dotnet build
dotnet run --project src/OpsCli.Cli -- project list --config samples/opscli.example.yml
```

## Como Executar Testes

```powershell
dotnet test OpsCli.sln
```

## Roadmap

- [x] Configuracao local via YAML
- [x] Cadastro e consulta de projetos
- [x] Validacao local de YAML
- [x] Verificacao de URLs
- [x] Status de repositorios Git
- [x] Project Check consolidado
- [ ] Git clone e git pull seguro
- [ ] Integracao com Azure DevOps Pipelines
- [ ] Consulta de Variable Groups / Libraries
- [ ] Diagnostico de execucao com falha
- [ ] Diff entre ambientes
- [ ] Consulta de recursos Azure
- [ ] Deploy preflight
- [ ] Relatorios Markdown e HTML
