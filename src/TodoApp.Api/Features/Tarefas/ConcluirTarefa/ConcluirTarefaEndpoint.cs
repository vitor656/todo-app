using Mediator;
using TodoApp.Api.Contracts;
using TodoApp.Api.Endpoints;
using TodoApp.Api.Messages;
using TodoApp.Application.Features.Tarefas.ConcluirTarefa;

namespace TodoApp.Api.Features.Tarefas.ConcluirTarefa;

public sealed class ConcluirTarefaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/tarefas/{id:guid}/concluir", HandleAsync)
            .WithTags("Tarefas")
            .WithSummary("Conclui uma tarefa pendente ou em andamento.")
            .Produces<ApiResponse<ConcluirTarefaOutput>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object?>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object?>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object?>>(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ConcluirTarefaInput(id), cancellationToken);

        return ResultHttpMapper.ToHttpResult(result, httpContext, ApiMessages.TarefaConcluida);
    }
}
