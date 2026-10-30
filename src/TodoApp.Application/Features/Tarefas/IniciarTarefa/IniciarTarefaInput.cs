using Mediator;
using TodoApp.Domain.Common;

namespace TodoApp.Application.Features.Tarefas.IniciarTarefa;

public sealed record IniciarTarefaInput(Guid Id) : ICommand<Result<IniciarTarefaOutput>>;
