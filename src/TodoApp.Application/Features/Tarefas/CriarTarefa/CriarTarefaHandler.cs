using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using TodoApp.Application.Abstractions;
using TodoApp.Domain.Common;

namespace TodoApp.Application.Features.Tarefas.CriarTarefa;

public sealed partial class CriarTarefaHandler(
    IValidator<CriarTarefaInput> validator,
    ITarefaRepository repository,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<CriarTarefaHandler> logger) : ICommandHandler<CriarTarefaInput, Result<CriarTarefaOutput>>
{
    public async ValueTask<Result<CriarTarefaOutput>> Handle(CriarTarefaInput command, CancellationToken cancellationToken)
    {
        LogCriando(logger, command.DataVencimento);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var erro = ParaErroDeValidacao(validationResult);
            LogFalha(logger, erro.Code, string.Join(", ", erro.FieldErrors.Select(f => f.Field)));
            return Result.Failure<CriarTarefaOutput>(erro);
        }

        var tarefaResult = command.ToDomain(clock.UtcNow);
        if (tarefaResult.IsFailure)
        {
            LogFalha(logger, tarefaResult.Error!.Code, string.Join(", ", tarefaResult.Error!.FieldErrors.Select(f => f.Field)));
            return Result.Failure<CriarTarefaOutput>(tarefaResult.Error!);
        }

        var tarefa = tarefaResult.Value;
        repository.Adicionar(tarefa);
        await unitOfWork.CommitAsync(cancellationToken);

        var agora = clock.UtcNow;
        LogCriada(logger, tarefa.Id, tarefa.DataVencimento);

        return Result.Success(CriarTarefaOutput.From(tarefa, agora));
    }

    private static Error ParaErroDeValidacao(ValidationResult validationResult)
    {
        var fieldErrors = validationResult.Errors
            .Select(e => new FieldError(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return Error.Validation("CriarTarefa.DadosInvalidos", "Os dados informados são inválidos.", fieldErrors);
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug, Message = "Criando tarefa. Vencimento: {DataVencimento}")]
    private static partial void LogCriando(ILogger logger, DateTime? dataVencimento);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Tarefa {TarefaId} criada. Vencimento: {DataVencimento}")]
    private static partial void LogCriada(ILogger logger, Guid tarefaId, DateTime? dataVencimento);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Warning, Message = "Falha ao criar tarefa: {Codigo}. Campos: {Campos}")]
    private static partial void LogFalha(ILogger logger, string codigo, string campos);
}
