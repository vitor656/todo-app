using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.IniciarTarefa;

public sealed record IniciarTarefaOutput(
    Guid Id,
    string Titulo,
    string? Descricao,
    StatusTarefa Status,
    DateTime? DataVencimento,
    DateTime CriadoEm,
    bool Vencida)
{
    public static IniciarTarefaOutput From(Tarefa tarefa, DateTime agora) => new(
        tarefa.Id,
        tarefa.Titulo,
        tarefa.Descricao,
        tarefa.Status,
        tarefa.DataVencimento,
        tarefa.CriadoEm,
        tarefa.EstaVencida(agora));
}
