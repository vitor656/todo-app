using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Abstractions;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Infrastructure.Persistence;

public sealed class TarefaRepository(AppDbContext context) : ITarefaRepository
{
    public void Adicionar(Tarefa tarefa) => context.Tarefas.Add(tarefa);

    public Task<Tarefa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Tarefas.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public void Remover(Tarefa tarefa) => context.Tarefas.Remove(tarefa);

    public async Task<IReadOnlyList<Tarefa>> ListarAsync(
        StatusTarefa? status,
        DateTime? vencimentoDe,
        DateTime? vencimentoAte,
        CancellationToken cancellationToken)
    {
        var query = context.Tarefas.AsNoTracking().AsQueryable();

        if (status is not null)
        {
            query = query.Where(t => t.Status == status);
        }

        if (vencimentoDe is not null)
        {
            query = query.Where(t => t.DataVencimento >= vencimentoDe);
        }

        if (vencimentoAte is not null)
        {
            query = query.Where(t => t.DataVencimento <= vencimentoAte);
        }

        return await query
            .OrderBy(t => t.DataVencimento.HasValue ? 0 : 1)
            .ThenBy(t => t.DataVencimento)
            .ToListAsync(cancellationToken);
    }
}
