using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using TodoApp.Application.Abstractions;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.IniciarTarefa;

public sealed partial class IniciarTarefaHandler(
    IValidator<IniciarTarefaInput> validator,
    ITarefaRepository repository,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<IniciarTarefaHandler> logger) : ICommandHandler<IniciarTarefaInput, Result<IniciarTarefaOutput>>
{
    public async ValueTask<Result<IniciarTarefaOutput>> Handle(IniciarTarefaInput command, CancellationToken cancellationToken)
    {
        LogIniciando(logger, command.Id);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<IniciarTarefaOutput>(ParaErroDeValidacao(validationResult));
        }

        var tarefa = await repository.ObterPorIdAsync(command.Id, cancellationToken);
        if (tarefa is null)
        {
            LogNaoEncontrada(logger, command.Id);
            return Result.Failure<IniciarTarefaOutput>(TarefaErros.NaoEncontrada());
        }

        var statusAnterior = tarefa.Status;
        var iniciarResult = tarefa.Iniciar();
        if (iniciarResult.IsFailure)
        {
            LogTransicaoInvalida(logger, tarefa.Id, statusAnterior);
            return Result.Failure<IniciarTarefaOutput>(iniciarResult.Error!);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        LogIniciada(logger, tarefa.Id, statusAnterior, tarefa.Status);

        return Result.Success(IniciarTarefaOutput.From(tarefa, clock.UtcNow));
    }

    private static Error ParaErroDeValidacao(ValidationResult validationResult)
    {
        var fieldErrors = validationResult.Errors
            .Select(e => new FieldError(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return Error.Validation("IniciarTarefa.DadosInvalidos", "Os dados informados são inválidos.", fieldErrors);
    }

    [LoggerMessage(EventId = 5001, Level = LogLevel.Debug, Message = "Iniciando tarefa {TarefaId}")]
    private static partial void LogIniciando(ILogger logger, Guid tarefaId);

    [LoggerMessage(EventId = 5002, Level = LogLevel.Information, Message = "Tarefa {TarefaId} iniciada. Status: {StatusAnterior} → {StatusNovo}")]
    private static partial void LogIniciada(ILogger logger, Guid tarefaId, StatusTarefa statusAnterior, StatusTarefa statusNovo);

    [LoggerMessage(EventId = 5003, Level = LogLevel.Warning, Message = "Tarefa {TarefaId} não encontrada ao iniciar")]
    private static partial void LogNaoEncontrada(ILogger logger, Guid tarefaId);

    [LoggerMessage(EventId = 5004, Level = LogLevel.Warning, Message = "Transição inválida ao iniciar a tarefa {TarefaId}. Status atual: {StatusAtual}")]
    private static partial void LogTransicaoInvalida(ILogger logger, Guid tarefaId, StatusTarefa statusAtual);
}
