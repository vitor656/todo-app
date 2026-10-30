using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Contracts;

namespace TodoApp.Api.ErrorHandling;

public sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
        {
            return true;
        }

        var metodo = httpContext.Request.Method;
        var caminho = httpContext.Request.Path.ToString();
        var traceId = ApiResponseFactory.GetTraceId(httpContext);

        int statusCode;
        string mensagem;

        switch (exception)
        {
            case BadHttpRequestException:
                statusCode = StatusCodes.Status400BadRequest;
                mensagem = "A requisição é inválida.";
                LogRequisicaoInvalida(logger, metodo, caminho, exception.Message);
                break;
            case DbUpdateConcurrencyException:
                statusCode = StatusCodes.Status409Conflict;
                mensagem = "Houve um conflito de concorrência ao processar a solicitação.";
                LogConflitoConcorrencia(logger, metodo, caminho);
                break;
            default:
                statusCode = StatusCodes.Status500InternalServerError;
                mensagem = "Ocorreu um erro interno. Informe o traceId ao suporte.";
                LogErroInesperado(logger, metodo, caminho, traceId, exception);
                break;
        }

        var response = ApiResponseFactory.Failure<object?>(mensagem, [], traceId);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    [LoggerMessage(EventId = 9001, Level = LogLevel.Error, Message = "Erro inesperado em {Metodo} {Caminho}. TraceId: {TraceId}")]
    private static partial void LogErroInesperado(ILogger logger, string metodo, string caminho, string traceId, Exception exception);

    [LoggerMessage(EventId = 9002, Level = LogLevel.Warning, Message = "Requisição inválida em {Metodo} {Caminho}: {Motivo}")]
    private static partial void LogRequisicaoInvalida(ILogger logger, string metodo, string caminho, string motivo);

    [LoggerMessage(EventId = 9003, Level = LogLevel.Warning, Message = "Conflito de concorrência em {Metodo} {Caminho}")]
    private static partial void LogConflitoConcorrencia(ILogger logger, string metodo, string caminho);
}
