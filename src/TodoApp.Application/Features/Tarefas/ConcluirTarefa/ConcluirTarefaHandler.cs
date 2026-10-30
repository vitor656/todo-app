using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using TodoApp.Application.Abstractions;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.ConcluirTarefa;

public sealed partial class ConcluirTarefaHandler(
    IValidator<ConcluirTarefaInput> validator,
    ITarefaRepository repository,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<ConcluirTarefaHandler> logger) : ICommandHandler<ConcluirTarefaInput, Result<ConcluirTarefaOutput>>
{
    public async ValueTask<Result<ConcluirTarefaOutput>> Handle(ConcluirTarefaInput command, CancellationToken cancellationToken)
    {
        LogConcluindo(logger, command.Id);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<ConcluirTarefaOutput>(ParaErroDeValidacao(validationResult));
        }

        var tarefa = await repository.ObterPorIdAsync(command.Id, cancellationToken);
        if (tarefa is null)
        {
            LogNaoEncontrada(logger, command.Id);
            return Result.Failure<ConcluirTarefaOutput>(TarefaErros.NaoEncontrada());
        }

        var statusAnterior = tarefa.Status;
        var concluirResult = tarefa.Concluir();
        if (concluirResult.IsFailure)
        {
            LogTransicaoInvalida(logger, tarefa.Id, statusAnterior);
            return Result.Failure<ConcluirTarefaOutput>(concluirResult.Error!);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        LogConcluida(logger, tarefa.Id, statusAnterior, tarefa.Status);

        return Result.Success(ConcluirTarefaOutput.From(tarefa, clock.UtcNow));
    }

    private static Error ParaErroDeValidacao(ValidationResult validationResult)
    {
        var fieldErrors = validationResult.Errors
            .Select(e => new FieldError(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return Error.Validation("ConcluirTarefa.DadosInvalidos", "Os dados informados são inválidos.", fieldErrors);
    }

    [LoggerMessage(EventId = 6001, Level = LogLevel.Debug, Message = "Concluindo tarefa {TarefaId}")]
    private static partial void LogConcluindo(ILogger logger, Guid tarefaId);

    [LoggerMessage(EventId = 6002, Level = LogLevel.Information, Message = "Tarefa {TarefaId} concluída. Status: {StatusAnterior} → {StatusNovo}")]
    private static partial void LogConcluida(ILogger logger, Guid tarefaId, StatusTarefa statusAnterior, StatusTarefa statusNovo);

    [LoggerMessage(EventId = 6003, Level = LogLevel.Warning, Message = "Tarefa {TarefaId} não encontrada ao concluir")]
    private static partial void LogNaoEncontrada(ILogger logger, Guid tarefaId);

    [LoggerMessage(EventId = 6004, Level = LogLevel.Warning, Message = "Transição inválida ao concluir a tarefa {TarefaId}. Status atual: {StatusAtual}")]
    private static partial void LogTransicaoInvalida(ILogger logger, Guid tarefaId, StatusTarefa statusAtual);
}
