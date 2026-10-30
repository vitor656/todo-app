using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using TodoApp.Application.Abstractions;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.AtualizarTarefa;

public sealed partial class AtualizarTarefaHandler(
    IValidator<AtualizarTarefaInput> validator,
    ITarefaRepository repository,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<AtualizarTarefaHandler> logger) : ICommandHandler<AtualizarTarefaInput, Result<AtualizarTarefaOutput>>
{
    public async ValueTask<Result<AtualizarTarefaOutput>> Handle(AtualizarTarefaInput command, CancellationToken cancellationToken)
    {
        LogAtualizando(logger, command.Id);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var erro = ParaErroDeValidacao(validationResult);
            LogFalha(logger, command.Id, erro.Code, string.Join(", ", erro.FieldErrors.Select(f => f.Field)));
            return Result.Failure<AtualizarTarefaOutput>(erro);
        }

        var tarefa = await repository.ObterPorIdAsync(command.Id, cancellationToken);
        if (tarefa is null)
        {
            LogNaoEncontrada(logger, command.Id);
            return Result.Failure<AtualizarTarefaOutput>(TarefaErros.NaoEncontrada());
        }

        var agora = clock.UtcNow;

        var detalhesResult = tarefa.AtualizarDetalhes(command.Titulo, command.Descricao);
        if (detalhesResult.IsFailure)
        {
            LogFalha(logger, command.Id, detalhesResult.Error!.Code, string.Join(", ", detalhesResult.Error!.FieldErrors.Select(f => f.Field)));
            return Result.Failure<AtualizarTarefaOutput>(detalhesResult.Error!);
        }

        var reagendarResult = tarefa.Reagendar(command.DataVencimento, agora);
        if (reagendarResult.IsFailure)
        {
            LogFalha(logger, command.Id, reagendarResult.Error!.Code, string.Join(", ", reagendarResult.Error!.FieldErrors.Select(f => f.Field)));
            return Result.Failure<AtualizarTarefaOutput>(reagendarResult.Error!);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        LogAtualizada(logger, tarefa.Id, "Titulo, Descricao, DataVencimento");

        return Result.Success(AtualizarTarefaOutput.From(tarefa, agora));
    }

    private static Error ParaErroDeValidacao(ValidationResult validationResult)
    {
        var fieldErrors = validationResult.Errors
            .Select(e => new FieldError(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return Error.Validation("AtualizarTarefa.DadosInvalidos", "Os dados informados são inválidos.", fieldErrors);
    }

    [LoggerMessage(EventId = 4001, Level = LogLevel.Debug, Message = "Atualizando tarefa {TarefaId}")]
    private static partial void LogAtualizando(ILogger logger, Guid tarefaId);

    [LoggerMessage(EventId = 4002, Level = LogLevel.Information, Message = "Tarefa {TarefaId} atualizada. Campos alterados: {Campos}")]
    private static partial void LogAtualizada(ILogger logger, Guid tarefaId, string campos);

    [LoggerMessage(EventId = 4003, Level = LogLevel.Warning, Message = "Tarefa {TarefaId} não encontrada ao atualizar")]
    private static partial void LogNaoEncontrada(ILogger logger, Guid tarefaId);

    [LoggerMessage(EventId = 4004, Level = LogLevel.Warning, Message = "Falha ao atualizar a tarefa {TarefaId}: {Codigo}. Campos: {Campos}")]
    private static partial void LogFalha(ILogger logger, Guid tarefaId, string codigo, string campos);
}
