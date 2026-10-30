using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.ConcluirTarefa;

public sealed record ConcluirTarefaOutput(
    Guid Id,
    string Titulo,
    string? Descricao,
    StatusTarefa Status,
    DateTime? DataVencimento,
    DateTime CriadoEm,
    bool Vencida)
{
    public static ConcluirTarefaOutput From(Tarefa tarefa, DateTime agora) => new(
        tarefa.Id,
        tarefa.Titulo,
        tarefa.Descricao,
        tarefa.Status,
        tarefa.DataVencimento,
        tarefa.CriadoEm,
        tarefa.EstaVencida(agora));
}
