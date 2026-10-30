using FluentValidation;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Application.Features.Tarefas.ListarTarefas;

public sealed class ListarTarefasInputValidator : AbstractValidator<ListarTarefasInput>
{
    private static readonly string[] StatusValidos = Enum.GetNames<StatusTarefa>();

    public ListarTarefasInputValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => status is null || StatusValidos.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"O status deve ser um dos seguintes valores: {string.Join(", ", StatusValidos)}.");

        RuleFor(x => x.VencimentoDe)
            .Must((input, vencimentoDe) => vencimentoDe is null || input.VencimentoAte is null || vencimentoDe <= input.VencimentoAte)
            .WithMessage("A data inicial do vencimento não pode ser posterior à data final.");
    }
}
