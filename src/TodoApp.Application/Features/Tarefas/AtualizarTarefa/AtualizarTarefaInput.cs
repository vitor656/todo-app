using Mediator;
using TodoApp.Domain.Common;

namespace TodoApp.Application.Features.Tarefas.AtualizarTarefa;

public sealed record AtualizarTarefaInput(Guid Id, string Titulo, string? Descricao, DateTime? DataVencimento)
    : ICommand<Result<AtualizarTarefaOutput>>;
