# TodoApp — API de Gestão de Tarefas

API REST para gestão de tarefas (to-do), construída em **.NET 10** com **Clean Architecture** e
**Vertical Slice**. Vocabulário do domínio (entidade, rotas, campos JSON, mensagens) em
português; termos técnicos (`Result`, `Error`, `Endpoint`, etc.) em inglês.

> **Instância em produção:** <https://todo-api-riq0.onrender.com/swagger/index.html>

## Sumário

1. [Visão geral](#1-visão-geral)
2. [Pré-requisitos](#2-pré-requisitos)
3. [Como rodar](#3-como-rodar)
4. [Testando pelo Swagger](#4-testando-pelo-swagger)
5. [Configuração](#5-configuração)
6. [Testes](#6-testes)
7. [Solução de problemas](#7-solução-de-problemas)
8. [Endpoints, estrutura e evoluções futuras](#8-endpoints-estrutura-e-evoluções-futuras)

## 1. Visão geral

A API permite criar, consultar, listar, atualizar, iniciar, concluir, reabrir e excluir tarefas.
Não há autenticação, múltiplos usuários, prioridade, etiquetas, subtarefas, anexos, notificações,
paginação nem remoção lógica — esse é o escopo deliberado desta versão.

**Stack:**

| Camada | Tecnologia |
| --- | --- |
| Api | ASP.NET Core Minimal APIs, OpenAPI nativo + Swashbuckle (interface) |
| Mediator | [Mediator](https://github.com/martinothamar/Mediator) (source generator, sem pipeline behaviors) |
| Validação | FluentValidation |
| Dados | Entity Framework Core + Npgsql (PostgreSQL) |
| Orquestração local | .NET Aspire (opcional) |
| Testes | xUnit, FluentAssertions, NSubstitute, coverlet |
| Observabilidade | OpenTelemetry (logs, traces, métricas) via Aspire ServiceDefaults |

**Camadas e regra de dependência:**

```text
┌─────────────────────────────────────────────────────────────┐
│ TodoApp.Api            (Minimal APIs, envelope, Swagger)      │
│   referencia → Application, Infrastructure, Domain,           │
│                 ServiceDefaults                                │
├─────────────────────────────────────────────────────────────┤
│ TodoApp.Infrastructure (EF Core, Npgsql, migrations)           │
│   referencia → Application, Domain                             │
├─────────────────────────────────────────────────────────────┤
│ TodoApp.Application    (Inputs/Outputs, Handlers, Validators)  │
│   referencia → Domain                                          │
├─────────────────────────────────────────────────────────────┤
│ TodoApp.Domain         (Tarefa, Result, Error — sem pacotes)   │
└─────────────────────────────────────────────────────────────┘

TodoApp.AppHost  → referencia apenas a Api (orquestra Postgres + Api via Aspire)
TodoApp.ServiceDefaults → referenciado só pela Api (OpenTelemetry, health checks)
```

A seta "referencia →" sempre aponta para dentro: o Domain não conhece nenhuma outra camada, e cada
camada externa só conhece as internas a ela.

## 2. Pré-requisitos

- **.NET 10 SDK** — confira com `dotnet --version` (precisa reportar `10.0.x` ou superior).
- **Docker** — usado para o PostgreSQL (em qualquer uma das opções) e para a imagem da Api
  (Opção C). Confira com `docker --version`.

Nenhum workload ou ferramenta específica do **Aspire** precisa ser instalado: ele chega por
pacotes NuGet comuns. A Opção A (a mais simples) nem o utiliza.

## 3. Como rodar

As três opções abaixo sobem a mesma Api, com o mesmo schema de banco. Escolha uma.

### Opção A — sem Aspire (caminho principal)

```bash
docker run --name todo-postgres -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=tododb -p 5432:5432 -d postgres:17

dotnet run --project src/TodoApp.Api
```

A connection string vem do `appsettings.Development.json`, que já usa as mesmas credenciais do
comando acima (`Host=localhost;Port=5432;Database=tododb;Username=postgres;Password=postgres`).
Em Development, a Api aplica as migrations pendentes automaticamente na inicialização.

**Swagger:** <http://localhost:5080/swagger>

### Opção B — com Aspire

```bash
dotnet run --project src/TodoApp.AppHost
```

Sobe o PostgreSQL (com pgAdmin) em container e a Api, e imprime no console o endereço do
dashboard do Aspire (logs, traces e métricas). Exige o Docker em execução.

**Swagger:** <http://localhost:5080/swagger>

### Opção C — imagem Docker

```bash
docker network create todo-net

docker run --name todo-postgres --network todo-net -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=tododb -d postgres:17

docker build -t todo-api .

docker run --rm -p 8080:8080 --network todo-net -e ConnectionStrings__Default="Host=todo-postgres;Port=5432;Database=tododb;Username=postgres;Password=postgres" -e Database__MigrateOnStartup=true -e Swagger__Habilitado=true todo-api
```

- `ConnectionStrings__Default`: obrigatória — a Api falha na inicialização, com mensagem clara, se
  ela estiver vazia.
- `Database__MigrateOnStartup=true`: necessária apenas na primeira execução contra um banco vazio;
  em Production, por padrão, as migrations automáticas vêm desligadas.
- `Swagger__Habilitado=true`: necessária porque, em Production (ambiente padrão da imagem), o
  Swagger vem desligado por padrão.

**Swagger:** <http://localhost:8080/swagger>

## 4. Testando pelo Swagger

1. Abra `/swagger` (endereço da opção escolhida).
2. Expanda `POST /api/v1/tarefas`, clique em **Try it out** e execute com um corpo como:

   ```json
   { "titulo": "Pagar a conta de luz", "descricao": "Vencimento no fim do mês", "dataVencimento": null }
   ```

   A resposta (201) traz o envelope padrão, com o `id` gerado em `data.id`.
3. Copie esse `id` e use-o em `GET /api/v1/tarefas/{id}`, `PUT /api/v1/tarefas/{id}`,
   `POST /api/v1/tarefas/{id}/iniciar`, `/concluir`, `/reabrir` e `DELETE /api/v1/tarefas/{id}`.

Toda resposta segue o mesmo envelope:

```json
{
  "success": true,
  "message": "Tarefa criada com sucesso.",
  "data": { "id": "0f8f6a52-3c1e-4d0b-9a77-2a5d3b1c9e10", "titulo": "Pagar a conta de luz", "status": "Pendente", "vencida": false },
  "errors": [],
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736"
}
```

### Exemplos com curl

Criar:

```bash
curl -X POST http://localhost:5080/api/v1/tarefas \
  -H "Content-Type: application/json" \
  -d '{"titulo":"Pagar a conta de luz","descricao":null,"dataVencimento":null}'
```

Obter:

```bash
curl http://localhost:5080/api/v1/tarefas/{id}
```

Listar com filtros:

```bash
curl "http://localhost:5080/api/v1/tarefas?status=Pendente&vencimentoDe=2026-01-01T00:00:00Z&vencimentoAte=2026-12-31T23:59:59Z"
```

Erro de validação (400) — título vazio:

```bash
curl -X POST http://localhost:5080/api/v1/tarefas \
  -H "Content-Type: application/json" \
  -d '{"titulo":"","descricao":null,"dataVencimento":null}'
```

```json
{"success":false,"message":"Os dados informados são inválidos.","data":null,"errors":[{"field":"Titulo","message":"O título da tarefa é obrigatório."}],"traceId":"..."}
```

Transição inválida (409) — iniciar uma tarefa já iniciada:

```bash
curl -X POST http://localhost:5080/api/v1/tarefas/{id}/iniciar
curl -X POST http://localhost:5080/api/v1/tarefas/{id}/iniciar
```

```json
{"success":false,"message":"Não é possível iniciar a tarefa porque o status atual é Em Andamento.","data":null,"errors":[],"traceId":"..."}
```

## 5. Configuração

| Chave | Base (`appsettings.json`) | Development | Staging | Production |
| --- | --- | --- | --- | --- |
| `ConnectionStrings:Default` | vazia | `Host=localhost;Port=5432;Database=tododb;Username=postgres;Password=postgres` | vazia (via `ConnectionStrings__Default`) | vazia (via `ConnectionStrings__Default`) |
| `Database:MigrateOnStartup` | `false` | `true` | `false` | `false` |
| `Swagger:Habilitado` | `true` | `true` | `true` | `false` |
| `Logging:LogLevel:Default` | `Information` | `Debug` | `Information` | `Information` |

O ambiente é escolhido por `ASPNETCORE_ENVIRONMENT` (`Development`, `Staging` ou `Production`).
Variáveis de ambiente usam `__` (dois underscores) no lugar de `:` — por exemplo,
`ConnectionStrings__Default` corresponde a `ConnectionStrings:Default`.

As credenciais do `appsettings.Development.json` (`postgres`/`postgres`) são exclusivas de uso
local e não devem ser usadas fora da sua máquina.

## 6. Testes

```bash
dotnet test
```

Com relatório de cobertura (restrito a `TodoApp.Domain` e `TodoApp.Application`, mínimo de 80%):

```bash
dotnet test tests/TodoApp.Tests --collect:"XPlat Code Coverage" --settings tests/TodoApp.Tests/coverlet.runsettings --results-directory ./coverage-results
```

O relatório (`coverage.cobertura.xml`) fica em `coverage-results/<guid>/`. Há apenas testes
unitários neste projeto — nenhum teste de integração.

## 7. Solução de problemas

- **Porta 5432 ou 5080 ocupada:** outro processo já está usando a porta. Pare-o ou ajuste a porta
  publicada no `docker run` (ex.: `-p 5433:5432`) e a connection string correspondente.
- **Docker parado:** as opções A, B e C dependem do Docker em execução para o PostgreSQL (e, na
  opção C, para a própria imagem da Api). Inicie o Docker Desktop (ou o daemon) antes de rodar os
  comandos.
- **Erro `A connection string 'ConnectionStrings:Default' não foi configurada.`:** a Api falha de
  propósito na inicialização quando a connection string está vazia. Confira o
  `appsettings.{Ambiente}.json` ou a variável `ConnectionStrings__Default`.
- **Tabela `tarefas` não existe:** confirme que `Database:MigrateOnStartup` está `true` no
  ambiente em uso, ou aplique as migrations manualmente com
  `dotnet ef database update --project src/TodoApp.Infrastructure --startup-project src/TodoApp.Infrastructure`.
- **Recomeçar do zero:**

  ```bash
  docker rm -f todo-postgres
  docker network rm todo-net
  ```

## 8. Endpoints, estrutura e evoluções futuras

### Endpoints (prefixo `/api/v1`)

| Rota | Sucesso | Erros possíveis |
| --- | --- | --- |
| `POST /tarefas` | 201 | 400 |
| `GET /tarefas/{id}` | 200 | 400, 404 |
| `GET /tarefas?status=&vencimentoDe=&vencimentoAte=` | 200 | 400 |
| `PUT /tarefas/{id}` | 200 | 400, 404 |
| `POST /tarefas/{id}/iniciar` | 200 | 400, 404, 409 |
| `POST /tarefas/{id}/concluir` | 200 | 400, 404, 409 |
| `POST /tarefas/{id}/reabrir` | 200 | 400, 404, 409 |
| `DELETE /tarefas/{id}` | 200 | 400, 404 |

### Estrutura de pastas

```text
src/
  TodoApp.Domain/            Tarefa, StatusTarefa, TarefaErros, Result, Error
  TodoApp.Application/       Abstractions + Features/Tarefas/<CasoDeUso>/
  TodoApp.Infrastructure/    Persistence (DbContext, Repository, UnitOfWork, Migrations)
  TodoApp.Api/               Contracts, ErrorHandling, Endpoints, Features/Tarefas/<CasoDeUso>/
  TodoApp.ServiceDefaults/   OpenTelemetry, health checks, resiliência
  TodoApp.AppHost/           Orquestração local via Aspire
tests/
  TodoApp.Tests/             Domain/, Application/, Api/
docs/adr/                    Registro das decisões arquiteturais
```

### Evoluções futuras (fora de escopo nesta versão)

- Paginação na listagem de tarefas.
- Autenticação e suporte a múltiplos usuários.
