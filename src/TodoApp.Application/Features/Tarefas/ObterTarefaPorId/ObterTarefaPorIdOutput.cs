using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.ObterTarefaPorId;

public sealed record ObterTarefaPorIdOutput(
    Guid Id,
    string Titulo,
    string? Descricao,
    StatusTarefa Status,
    DateTime? DataVencimento,
    DateTime CriadoEm,
    bool Vencida)
{
    public static ObterTarefaPorIdOutput From(Tarefa tarefa, DateTime agora) => new(
        tarefa.Id,
        tarefa.Titulo,
        tarefa.Descricao,
        tarefa.Status,
        tarefa.DataVencimento,
        tarefa.CriadoEm,
        tarefa.EstaVencida(agora));
}
