using Mediator;
using TodoApp.Domain.Common;

namespace TodoApp.Application.Features.Tarefas.ReabrirTarefa;

public sealed record ReabrirTarefaInput(Guid Id) : ICommand<Result<ReabrirTarefaOutput>>;
