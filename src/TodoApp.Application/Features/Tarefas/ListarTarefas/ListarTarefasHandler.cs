using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using TodoApp.Application.Abstractions;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.ListarTarefas;

public sealed partial class ListarTarefasHandler(
    IValidator<ListarTarefasInput> validator,
    ITarefaRepository repository,
    IClock clock,
    ILogger<ListarTarefasHandler> logger) : IQueryHandler<ListarTarefasInput, Result<IReadOnlyList<ListarTarefasOutput>>>
{
    public async ValueTask<Result<IReadOnlyList<ListarTarefasOutput>>> Handle(ListarTarefasInput query, CancellationToken cancellationToken)
    {
        LogListando(logger, query.Status, query.VencimentoDe, query.VencimentoAte);

        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            var erro = ParaErroDeValidacao(validationResult);
            LogFiltrosInvalidos(logger, string.Join(", ", erro.FieldErrors.Select(f => f.Field)));
            return Result.Failure<IReadOnlyList<ListarTarefasOutput>>(erro);
        }

        StatusTarefa? status = query.Status is null ? null : Enum.Parse<StatusTarefa>(query.Status, ignoreCase: true);

        var agora = clock.UtcNow;
        var tarefas = await repository.ListarAsync(status, query.VencimentoDe, query.VencimentoAte, cancellationToken);
        IReadOnlyList<ListarTarefasOutput> output = tarefas.Select(t => ListarTarefasOutput.From(t, agora)).ToList();

        LogListagemConcluida(logger, output.Count);

        return Result.Success(output);
    }

    private static Error ParaErroDeValidacao(ValidationResult validationResult)
    {
        var fieldErrors = validationResult.Errors
            .Select(e => new FieldError(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return Error.Validation("ListarTarefas.FiltrosInvalidos", "Os filtros informados são inválidos.", fieldErrors);
    }

    [LoggerMessage(EventId = 3001, Level = LogLevel.Debug, Message = "Listando tarefas. Status: {Status}, VencimentoDe: {VencimentoDe}, VencimentoAte: {VencimentoAte}")]
    private static partial void LogListando(ILogger logger, string? status, DateTime? vencimentoDe, DateTime? vencimentoAte);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Information, Message = "Listagem concluída. Quantidade: {Quantidade}")]
    private static partial void LogListagemConcluida(ILogger logger, int quantidade);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Warning, Message = "Filtros inválidos na listagem. Campos: {Campos}")]
    private static partial void LogFiltrosInvalidos(ILogger logger, string campos);
}
