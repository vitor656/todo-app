using Mediator;
using TodoApp.Api.Contracts;
using TodoApp.Api.Endpoints;
using TodoApp.Api.Messages;
using TodoApp.Application.Features.Tarefas.IniciarTarefa;

namespace TodoApp.Api.Features.Tarefas.IniciarTarefa;

public sealed class IniciarTarefaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/tarefas/{id:guid}/iniciar", HandleAsync)
            .WithTags("Tarefas")
            .WithSummary("Inicia uma tarefa pendente.")
            .Produces<ApiResponse<IniciarTarefaOutput>>(StatusCodes.Status200OK)
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
        var result = await mediator.Send(new IniciarTarefaInput(id), cancellationToken);

        return ResultHttpMapper.ToHttpResult(result, httpContext, ApiMessages.TarefaIniciada);
    }
}
