using FluentValidation;

namespace TodoApp.Application.Features.Tarefas.ObterTarefaPorId;

public sealed class ObterTarefaPorIdInputValidator : AbstractValidator<ObterTarefaPorIdInput>
{
    public ObterTarefaPorIdInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("O identificador da tarefa é obrigatório.");
    }
}
