using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using TodoApp.Application.Abstractions;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.ReabrirTarefa;

public sealed partial class ReabrirTarefaHandler(
    IValidator<ReabrirTarefaInput> validator,
    ITarefaRepository repository,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<ReabrirTarefaHandler> logger) : ICommandHandler<ReabrirTarefaInput, Result<ReabrirTarefaOutput>>
{
    public async ValueTask<Result<ReabrirTarefaOutput>> Handle(ReabrirTarefaInput command, CancellationToken cancellationToken)
    {
        LogReabrindo(logger, command.Id);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<ReabrirTarefaOutput>(ParaErroDeValidacao(validationResult));
        }

        var tarefa = await repository.ObterPorIdAsync(command.Id, cancellationToken);
        if (tarefa is null)
        {
            LogNaoEncontrada(logger, command.Id);
            return Result.Failure<ReabrirTarefaOutput>(TarefaErros.NaoEncontrada());
        }

        var statusAnterior = tarefa.Status;
        var reabrirResult = tarefa.Reabrir();
        if (reabrirResult.IsFailure)
        {
            LogTransicaoInvalida(logger, tarefa.Id, statusAnterior);
            return Result.Failure<ReabrirTarefaOutput>(reabrirResult.Error!);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        LogReaberta(logger, tarefa.Id, statusAnterior, tarefa.Status);

        return Result.Success(ReabrirTarefaOutput.From(tarefa, clock.UtcNow));
    }

    private static Error ParaErroDeValidacao(ValidationResult validationResult)
    {
        var fieldErrors = validationResult.Errors
            .Select(e => new FieldError(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return Error.Validation("ReabrirTarefa.DadosInvalidos", "Os dados informados são inválidos.", fieldErrors);
    }

    [LoggerMessage(EventId = 7001, Level = LogLevel.Debug, Message = "Reabrindo tarefa {TarefaId}")]
    private static partial void LogReabrindo(ILogger logger, Guid tarefaId);

    [LoggerMessage(EventId = 7002, Level = LogLevel.Information, Message = "Tarefa {TarefaId} reaberta. Status: {StatusAnterior} → {StatusNovo}")]
    private static partial void LogReaberta(ILogger logger, Guid tarefaId, StatusTarefa statusAnterior, StatusTarefa statusNovo);

    [LoggerMessage(EventId = 7003, Level = LogLevel.Warning, Message = "Tarefa {TarefaId} não encontrada ao reabrir")]
    private static partial void LogNaoEncontrada(ILogger logger, Guid tarefaId);

    [LoggerMessage(EventId = 7004, Level = LogLevel.Warning, Message = "Transição inválida ao reabrir a tarefa {TarefaId}. Status atual: {StatusAtual}")]
    private static partial void LogTransicaoInvalida(ILogger logger, Guid tarefaId, StatusTarefa statusAtual);
}
