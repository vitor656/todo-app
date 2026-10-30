# 0003. Result em vez de exceções para regras de negócio

## Status

Aceito

## Contexto

Lançar exceções para representar falhas esperadas (validação, não encontrado, conflito de
transição de status) misturaria controle de fluxo normal do domínio com o mecanismo reservado
para erros verdadeiramente inesperados, e tornaria o custo de cada falha (stack unwinding)
desproporcional à frequência com que elas ocorrem numa API de tarefas.

## Decisão

Regras de negócio no Domain e na Application nunca lançam exceções: todo método que pode falhar
devolve `Result` ou `Result<T>`, com um `Error` tipado (`Validation`, `NotFound` ou `Conflict`).
Exceções ficam reservadas para falhas realmente inesperadas (infraestrutura, bugs), tratadas uma
única vez pelo `GlobalExceptionHandler` na Api.

## Consequências

- O tipo de retorno de cada método deixa explícito que ele pode falhar, e o compilador força o
  chamador a lidar com os dois casos.
- A Api tem um único lugar (`ResultHttpMapper`) que traduz `Error` em status HTTP, mantendo a
  Application sem qualquer conhecimento do protocolo HTTP.
- Em compensação, o código tem mais verificações explícitas de `IsSuccess`/`IsFailure` do que teria
  com `try/catch`.
