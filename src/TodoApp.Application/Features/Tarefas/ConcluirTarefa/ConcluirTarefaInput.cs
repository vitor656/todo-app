using Mediator;
using TodoApp.Domain.Common;

namespace TodoApp.Application.Features.Tarefas.ConcluirTarefa;

public sealed record ConcluirTarefaInput(Guid Id) : ICommand<Result<ConcluirTarefaOutput>>;
