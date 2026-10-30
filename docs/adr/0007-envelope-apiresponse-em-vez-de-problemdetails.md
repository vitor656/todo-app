# 0007. Envelope ApiResponse&lt;T&gt; em vez de ProblemDetails

## Status

Aceito

## Contexto

O ASP.NET Core oferece `ProblemDetails` como formato padrão para respostas de erro, mas ele não
teria como representar, no mesmo formato, as respostas de sucesso — e o objetivo aqui é que toda
resposta da Api, de sucesso ou de erro, siga exatamente o mesmo formato, incluindo `traceId` e uma
mensagem em português voltada ao cliente da API.

## Decisão

Toda resposta de `/api/**` usa o envelope `ApiResponse<T>` (`success`, `message`, `data`, `errors`,
`traceId`), criado apenas pela `ApiResponseFactory` e escrito apenas pelos endpoints (via
`ResultHttpMapper`), pelo `GlobalExceptionHandler` e pelo `UseStatusCodePages`. Nenhum endpoint usa
`Results.Ok(dto)`, `Results.NotFound()` nem `ProblemDetails` diretamente.

## Consequências

- Clientes da API lidam com um único formato de resposta, independentemente do status HTTP.
- A Application não precisa conhecer o envelope: ela devolve `Result`, e só a Api sabe transformar
  isso em `ApiResponse<T>`.
- Ferramentas e middlewares do ecossistema ASP.NET Core que assumem `ProblemDetails` (como
  `AddProblemDetails`) não são usados para gerar corpo de resposta; eles continuam registrados
  apenas como pré-requisito interno do middleware `UseExceptionHandler`, nunca aparecendo no corpo
  enviado ao cliente.
