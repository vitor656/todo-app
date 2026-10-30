using Mediator;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.CriarTarefa;

public sealed record CriarTarefaInput(string Titulo, string? Descricao, DateTime? DataVencimento)
    : ICommand<Result<CriarTarefaOutput>>
{
    public Result<Tarefa> ToDomain(DateTime agora) => Tarefa.Criar(Titulo, Descricao, DataVencimento, agora);
}
