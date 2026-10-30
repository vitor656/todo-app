# 0001. Vertical Slice dentro de Clean Architecture

## Status

Aceito

## Contexto

O projeto segue Clean Architecture nas camadas (Domain, Application, Infrastructure, Api), mas uma
organização puramente horizontal dentro de Application e Api (uma pasta "Handlers", outra
"Validators", outra "Endpoints") dificulta encontrar tudo que pertence a um caso de uso e aumenta
o acoplamento acidental entre casos de uso diferentes.

## Decisão

Dentro das camadas Application e Api, cada caso de uso (por exemplo, `CriarTarefa`) é uma fatia
autocontida em `Features/Tarefas/<CasoDeUso>/`, com seu Input, Output, Validator e Handler (na
Application) e seu Endpoint (na Api). Fatias não referenciam namespaces de outras fatias.

## Consequências

- Adicionar, remover ou entender um caso de uso exige olhar uma única pasta.
- Algum código se repete entre fatias (por exemplo, Outputs parecidos), o que é aceito
  deliberadamente em troca do isolamento.
- A regra de dependência entre camadas (Domain ← Application ← Infrastructure ← Api) continua
  valendo integralmente; a Vertical Slice organiza apenas o que fica dentro de cada camada.
