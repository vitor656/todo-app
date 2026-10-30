using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using TodoApp.Application.Abstractions;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.ExcluirTarefa;

public sealed partial class ExcluirTarefaHandler(
    IValidator<ExcluirTarefaInput> validator,
    ITarefaRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ExcluirTarefaHandler> logger) : ICommandHandler<ExcluirTarefaInput, Result>
{
    public async ValueTask<Result> Handle(ExcluirTarefaInput command, CancellationToken cancellationToken)
    {
        LogExcluindo(logger, command.Id);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(ParaErroDeValidacao(validationResult));
        }

        var tarefa = await repository.ObterPorIdAsync(command.Id, cancellationToken);
        if (tarefa is null)
        {
            LogNaoEncontrada(logger, command.Id);
            return Result.Failure(TarefaErros.NaoEncontrada());
        }

        repository.Remover(tarefa);
        await unitOfWork.CommitAsync(cancellationToken);

        LogExcluida(logger, tarefa.Id);

        return Result.Success();
    }

    private static Error ParaErroDeValidacao(ValidationResult validationResult)
    {
        var fieldErrors = validationResult.Errors
            .Select(e => new FieldError(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return Error.Validation("ExcluirTarefa.DadosInvalidos", "Os dados informados são inválidos.", fieldErrors);
    }

    [LoggerMessage(EventId = 8001, Level = LogLevel.Debug, Message = "Excluindo tarefa {TarefaId}")]
    private static partial void LogExcluindo(ILogger logger, Guid tarefaId);

    [LoggerMessage(EventId = 8002, Level = LogLevel.Information, Message = "Tarefa {TarefaId} excluída")]
    private static partial void LogExcluida(ILogger logger, Guid tarefaId);

    [LoggerMessage(EventId = 8003, Level = LogLevel.Warning, Message = "Tarefa {TarefaId} não encontrada ao excluir")]
    private static partial void LogNaoEncontrada(ILogger logger, Guid tarefaId);
}
