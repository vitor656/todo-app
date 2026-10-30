using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Abstractions;

public interface ITarefaRepository
{
    void Adicionar(Tarefa tarefa);

    Task<Tarefa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    void Remover(Tarefa tarefa);

    Task<IReadOnlyList<Tarefa>> ListarAsync(
        StatusTarefa? status,
        DateTime? vencimentoDe,
        DateTime? vencimentoAte,
        CancellationToken cancellationToken);
}
