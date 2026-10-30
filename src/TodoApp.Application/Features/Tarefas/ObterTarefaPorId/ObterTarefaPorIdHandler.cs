using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using TodoApp.Application.Abstractions;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.ObterTarefaPorId;

public sealed partial class ObterTarefaPorIdHandler(
    IValidator<ObterTarefaPorIdInput> validator,
    ITarefaRepository repository,
    IClock clock,
    ILogger<ObterTarefaPorIdHandler> logger) : IQueryHandler<ObterTarefaPorIdInput, Result<ObterTarefaPorIdOutput>>
{
    public async ValueTask<Result<ObterTarefaPorIdOutput>> Handle(ObterTarefaPorIdInput query, CancellationToken cancellationToken)
    {
        LogConsultando(logger, query.Id);

        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<ObterTarefaPorIdOutput>(ParaErroDeValidacao(validationResult));
        }

        var tarefa = await repository.ObterPorIdAsync(query.Id, cancellationToken);
        if (tarefa is null)
        {
            LogNaoEncontrada(logger, query.Id);
            return Result.Failure<ObterTarefaPorIdOutput>(TarefaErros.NaoEncontrada());
        }

        LogEncontrada(logger, tarefa.Id, tarefa.Status);

        return Result.Success(ObterTarefaPorIdOutput.From(tarefa, clock.UtcNow));
    }

    private static Error ParaErroDeValidacao(ValidationResult validationResult)
    {
        var fieldErrors = validationResult.Errors
            .Select(e => new FieldError(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return Error.Validation("ObterTarefaPorId.DadosInvalidos", "Os dados informados são inválidos.", fieldErrors);
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Debug, Message = "Consultando tarefa {TarefaId}")]
    private static partial void LogConsultando(ILogger logger, Guid tarefaId);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Debug, Message = "Tarefa {TarefaId} encontrada com status {Status}")]
    private static partial void LogEncontrada(ILogger logger, Guid tarefaId, StatusTarefa status);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Warning, Message = "Tarefa {TarefaId} não encontrada")]
    private static partial void LogNaoEncontrada(ILogger logger, Guid tarefaId);
}
