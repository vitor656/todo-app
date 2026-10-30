using Mediator;
using TodoApp.Api.Contracts;
using TodoApp.Api.Endpoints;
using TodoApp.Api.Messages;
using TodoApp.Api.Serialization;
using TodoApp.Application.Features.Tarefas.ListarTarefas;

namespace TodoApp.Api.Features.Tarefas.ListarTarefas;

public sealed class ListarTarefasEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/tarefas", HandleAsync)
            .WithTags("Tarefas")
            .WithSummary("Lista as tarefas, com filtros opcionais por status e vencimento.")
            .Produces<ApiResponse<IReadOnlyList<ListarTarefasOutput>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object?>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        string? status,
        DateTime? vencimentoDe,
        DateTime? vencimentoAte,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var input = new ListarTarefasInput(
            status,
            vencimentoDe is null ? null : UtcDateTimeJsonConverter.ParaUtc(vencimentoDe.Value),
            vencimentoAte is null ? null : UtcDateTimeJsonConverter.ParaUtc(vencimentoAte.Value));
        var result = await mediator.Send(input, cancellationToken);

        return ResultHttpMapper.ToHttpResult(result, httpContext, ApiMessages.TarefasListadas);
    }
}
