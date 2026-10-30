using Mediator;
using TodoApp.Api.Contracts;
using TodoApp.Api.Endpoints;
using TodoApp.Api.Messages;
using TodoApp.Application.Features.Tarefas.AtualizarTarefa;

namespace TodoApp.Api.Features.Tarefas.AtualizarTarefa;

public sealed class AtualizarTarefaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/tarefas/{id:guid}", HandleAsync)
            .WithTags("Tarefas")
            .WithSummary("Atualiza o título, a descrição e o vencimento de uma tarefa.")
            .Produces<ApiResponse<AtualizarTarefaOutput>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object?>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object?>>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        AtualizarTarefaRequest body,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var input = new AtualizarTarefaInput(id, body.Titulo, body.Descricao, body.DataVencimento);
        var result = await mediator.Send(input, cancellationToken);

        return ResultHttpMapper.ToHttpResult(result, httpContext, ApiMessages.TarefaAtualizada);
    }
}

public sealed record AtualizarTarefaRequest(string Titulo, string? Descricao, DateTime? DataVencimento);
