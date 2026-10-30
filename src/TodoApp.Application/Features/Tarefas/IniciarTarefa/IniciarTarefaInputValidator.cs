using FluentValidation;

namespace TodoApp.Application.Features.Tarefas.IniciarTarefa;

public sealed class IniciarTarefaInputValidator : AbstractValidator<IniciarTarefaInput>
{
    public IniciarTarefaInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("O identificador da tarefa é obrigatório.");
    }
}
