using Mediator;
using TodoApp.Api.Contracts;
using TodoApp.Api.Endpoints;
using TodoApp.Api.Messages;
using TodoApp.Application.Features.Tarefas.ObterTarefaPorId;

namespace TodoApp.Api.Features.Tarefas.ObterTarefaPorId;

public sealed class ObterTarefaPorIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/tarefas/{id:guid}", HandleAsync)
            .WithTags("Tarefas")
            .WithSummary("Consulta uma tarefa pelo identificador.")
            .Produces<ApiResponse<ObterTarefaPorIdOutput>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object?>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object?>>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ObterTarefaPorIdInput(id), cancellationToken);

        return ResultHttpMapper.ToHttpResult(result, httpContext, ApiMessages.TarefaObtida);
    }
}
