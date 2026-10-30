# 0008. Migrations aplicadas na inicialização

## Status

Aceito

## Contexto

Exigir que alguém rode `dotnet ef database update` manualmente antes de cada execução local
adicionaria um passo extra fácil de esquecer, especialmente para quem está testando o projeto por
mantê-lo simples de rodar (via Aspire, via `dotnet run` direto, ou via a imagem Docker). Por outro
lado, aplicar migrations automaticamente sempre, em qualquer ambiente, é arriscado em produção.

## Decisão

`MigrateDatabaseAsync`, chamado no `Program.cs` logo após `app.Build()`, só aplica as migrations
pendentes quando `Database:MigrateOnStartup` é verdadeiro. Essa chave é `true` apenas em
Development; em Staging, em Production e na imagem Docker por padrão ela é `false`, exigindo uma
decisão explícita (variável de ambiente `Database__MigrateOnStartup=true`) para migrar
automaticamente fora do ambiente local.

## Consequências

- Em Development, `dotnet run` (com ou sem Aspire) sempre deixa o banco com o schema atualizado,
  sem passo manual.
- Em Staging e Production, migrations exigem uma ação deliberada, reduzindo o risco de uma
  migration destrutiva rodar sem supervisão durante um deploy.
- Testar a imagem Docker localmente com um banco vazio exige lembrar de passar
  `Database__MigrateOnStartup=true`, o que está documentado no README.
