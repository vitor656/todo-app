# 0002. Mediator sem pipeline behaviors

## Status

Aceito

## Contexto

Bibliotecas de Mediator costumam ser usadas junto de pipeline behaviors (validação, logging,
transação) para evitar repetição nos handlers. Isso facilita a vida no começo, mas esconde o
fluxo de cada caso de uso atrás de uma cadeia de decorators genérica, dificultando entender o que
realmente acontece ao ler um único handler.

## Decisão

Usar a biblioteca [Mediator](https://github.com/martinothamar/Mediator) apenas como despachante de
mensagens (`IMediator.Send`), sem `IPipelineBehavior` nem notificações. Cada handler chama
explicitamente o `IValidator<Input>`, a entidade de domínio e o `IUnitOfWork.CommitAsync`, sempre
na mesma ordem (validar, carregar, aplicar regra, comitar, mapear, logar).

## Consequências

- Todo handler é lido de cima a baixo sem indireção; não há "mágica" de pipeline para depurar.
- Passos comuns (como logar o início da operação) se repetem entre handlers, o que é aceito em
  nome da legibilidade.
- Novos cross-cutting concerns, se necessários no futuro, exigirão decisão explícita sobre onde
  adicioná-los — não há um behavior central para interceptá-los automaticamente.
