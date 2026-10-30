using Mediator;
using TodoApp.Api.Contracts;
using TodoApp.Api.Endpoints;
using TodoApp.Api.Messages;
using TodoApp.Application.Features.Tarefas.CriarTarefa;

namespace TodoApp.Api.Features.Tarefas.CriarTarefa;

public sealed class CriarTarefaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/tarefas", HandleAsync)
            .WithTags("Tarefas")
            .WithSummary("Cria uma nova tarefa.")
            .Produces<ApiResponse<CriarTarefaOutput>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object?>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        CriarTarefaInput input,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(input, cancellationToken);

        return ResultHttpMapper.ToCreatedResult(
            result,
            httpContext,
            ApiMessages.TarefaCriada,
            output => $"/api/v1/tarefas/{output.Id}");
    }
}
