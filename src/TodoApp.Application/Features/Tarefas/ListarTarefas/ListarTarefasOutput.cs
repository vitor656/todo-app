using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.ListarTarefas;

public sealed record ListarTarefasOutput(
    Guid Id,
    string Titulo,
    string? Descricao,
    StatusTarefa Status,
    DateTime? DataVencimento,
    DateTime CriadoEm,
    bool Vencida)
{
    public static ListarTarefasOutput From(Tarefa tarefa, DateTime agora) => new(
        tarefa.Id,
        tarefa.Titulo,
        tarefa.Descricao,
        tarefa.Status,
        tarefa.DataVencimento,
        tarefa.CriadoEm,
        tarefa.EstaVencida(agora));
}
