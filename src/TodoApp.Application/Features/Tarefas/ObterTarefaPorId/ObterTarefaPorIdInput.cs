using Mediator;
using TodoApp.Domain.Common;

namespace TodoApp.Application.Features.Tarefas.ObterTarefaPorId;

public sealed record ObterTarefaPorIdInput(Guid Id) : IQuery<Result<ObterTarefaPorIdOutput>>;
