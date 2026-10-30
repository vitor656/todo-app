# 0004. Repository e Unit of Work sobre o EF Core

## Status

Aceito

## Contexto

A Application não pode depender do EF Core diretamente (regra de dependência entre camadas), mas
precisa de um jeito de carregar, listar e persistir tarefas, e de controlar quando os comandos
realmente gravam no banco.

## Decisão

Definir `ITarefaRepository` e `IUnitOfWork` como abstrações na Application, implementadas na
Infrastructure sobre o `AppDbContext`. O repositório nunca comita: `Adicionar` e `Remover` apenas
alteram o `ChangeTracker`, e o `IUnitOfWork.CommitAsync` (que delega para
`DbContext.SaveChangesAsync`) é o único ponto de commit, chamado uma única vez por handler de
escrita, somente quando a operação é bem-sucedida.

## Consequências

- A Application permanece livre de EF Core, Npgsql e qualquer detalhe de persistência.
- Cada handler controla explicitamente se e quando comita, o que reforça o fluxo descrito na ADR
  0002 (sem pipeline automático de commit).
- Como há apenas uma entidade (`Tarefa`), um único repositório é suficiente nesta versão; um
  repositório genérico não traria benefício e contrariaria o princípio de simplicidade do projeto.
