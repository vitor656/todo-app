using Mediator;
using TodoApp.Domain.Common;

namespace TodoApp.Application.Features.Tarefas.ExcluirTarefa;

public sealed record ExcluirTarefaInput(Guid Id) : ICommand<Result>;
