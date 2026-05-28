# OpsCli

OpsCli e uma ferramenta de linha de comando em C#/.NET 10 para automatizar validacoes locais comuns em rotinas de DevOps.

Esta primeira versao nao integra com Azure DevOps nem Azure. O foco e uma base limpa, testavel e organizada para evoluir os comandos.

## Estrutura

- `src/OpsCli.Cli`: comandos e output do terminal.
- `src/OpsCli.Core`: modelos, interfaces e regras.
- `src/OpsCli.Infrastructure`: YAML, Git local e HTTP.
- `src/OpsCli.Tests`: testes unitarios xUnit.
- `samples/opscli.example.yml`: exemplo de configuracao.

## Comandos

```powershell
dotnet run --project src/OpsCli.Cli -- config init
dotnet run --project src/OpsCli.Cli -- project list
dotnet run --project src/OpsCli.Cli -- project show api-finops --env dev
dotnet run --project src/OpsCli.Cli -- repos exists
dotnet run --project src/OpsCli.Cli -- repos status
dotnet run --project src/OpsCli.Cli -- yaml validate-all
dotnet run --project src/OpsCli.Cli -- urls list --project api-finops --env dev
dotnet run --project src/OpsCli.Cli -- urls check --project api-finops --env dev
dotnet run --project src/OpsCli.Cli -- project check api-finops --env dev
```

Todos os comandos que leem configuracao aceitam `--config <path>`. Sem esse parametro, a CLI procura `opscli.yml` no diretorio atual.

## Build e testes

```powershell
dotnet build
dotnet test
```
