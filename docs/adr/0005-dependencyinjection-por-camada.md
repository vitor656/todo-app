# 0005. DependencyInjection.cs por camada

## Status

Aceito

## Contexto

Registrar serviços diretamente no `Program.cs` da Api acopla a composição da aplicação a detalhes
de cada camada (EF Core, FluentValidation, Mediator) e torna o `Program.cs` cada vez mais longo à
medida que o projeto cresce.

## Decisão

Cada projeto, exceto o Domain, expõe um único método de extensão em um `DependencyInjection.cs` na
sua raiz (`AddApplication`, `AddInfrastructure`, `AddApi`) que registra tudo que a camada precisa.
O `Program.cs` apenas encadeia essas chamadas, na ordem Application → Infrastructure → Api, sem
nenhum registro solto.

## Consequências

- O `Program.cs` serve como um índice de alto nível da composição da aplicação.
- Cada camada é responsável por conhecer suas próprias dependências de DI; a Api não precisa saber
  como a Infrastructure registra o `DbContext`, por exemplo.
- Testar a configuração de DI de uma camada isoladamente fica mais simples, caso isso seja
  necessário no futuro.
