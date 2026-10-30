using Mediator;
using TodoApp.Api.Contracts;
using TodoApp.Api.Endpoints;
using TodoApp.Api.Messages;
using TodoApp.Application.Features.Tarefas.ReabrirTarefa;

namespace TodoApp.Api.Features.Tarefas.ReabrirTarefa;

public sealed class ReabrirTarefaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/tarefas/{id:guid}/reabrir", HandleAsync)
            .WithTags("Tarefas")
            .WithSummary("Reabre uma tarefa concluída.")
            .Produces<ApiResponse<ReabrirTarefaOutput>>(StatusCodes.Status200OK)
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
        var result = await mediator.Send(new ReabrirTarefaInput(id), cancellationToken);

        return ResultHttpMapper.ToHttpResult(result, httpContext, ApiMessages.TarefaReaberta);
    }
}
