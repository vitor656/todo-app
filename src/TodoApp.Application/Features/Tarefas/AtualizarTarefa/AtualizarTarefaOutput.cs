using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.AtualizarTarefa;

public sealed record AtualizarTarefaOutput(
    Guid Id,
    string Titulo,
    string? Descricao,
    StatusTarefa Status,
    DateTime? DataVencimento,
    DateTime CriadoEm,
    bool Vencida)
{
    public static AtualizarTarefaOutput From(Tarefa tarefa, DateTime agora) => new(
        tarefa.Id,
        tarefa.Titulo,
        tarefa.Descricao,
        tarefa.Status,
        tarefa.DataVencimento,
        tarefa.CriadoEm,
        tarefa.EstaVencida(agora));
}
