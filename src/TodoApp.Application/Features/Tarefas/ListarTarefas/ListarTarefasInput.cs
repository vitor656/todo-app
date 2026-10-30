using Mediator;
using TodoApp.Domain.Common;

namespace TodoApp.Application.Features.Tarefas.ListarTarefas;

public sealed record ListarTarefasInput(string? Status, DateTime? VencimentoDe, DateTime? VencimentoAte)
    : IQuery<Result<IReadOnlyList<ListarTarefasOutput>>>;
