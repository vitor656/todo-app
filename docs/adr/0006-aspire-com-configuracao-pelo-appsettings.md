# 0006. Aspire com configuração pelo appsettings

## Status

Aceito

## Contexto

O Aspire permite que o AppHost injete connection strings e outras configurações nos projetos via
`WithReference`, mas isso tornaria a Api dependente do Aspire para funcionar: ela só obteria a
connection string corretamente quando executada pelo AppHost, e exigiria um caminho de
configuração totalmente diferente para quem apenas roda `dotnet run --project src/TodoApp.Api` ou
a imagem Docker.

## Decisão

O AppHost sobe o PostgreSQL (e o pgAdmin) em container, mas não usa `WithReference` para a Api e
não injeta nenhuma variável de configuração nela. A Api obtém a connection string e as demais
chaves exclusivamente do `appsettings.{Ambiente}.json` e de variáveis de ambiente, com as mesmas
credenciais fixas usadas pelo container do AppHost em Development.

## Consequências

- A Api se comporta de forma idêntica nos três caminhos de execução descritos no README (sem
  Aspire, com Aspire, e via imagem Docker), já que nenhum deles depende de injeção do Aspire.
- O Aspire fica restrito ao papel de conveniência local (orquestrar containers, mostrar
  dashboard), nunca de fonte de configuração obrigatória.
- Trocar a connection string de Development (por exemplo, para outra porta de PostgreSQL) exige
  atualizar os dois lugares — o `appsettings.Development.json` e o AppHost — já que eles não
  compartilham a mesma fonte.
