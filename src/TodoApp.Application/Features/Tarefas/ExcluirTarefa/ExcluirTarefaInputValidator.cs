using FluentValidation;

namespace TodoApp.Application.Features.Tarefas.ExcluirTarefa;

public sealed class ExcluirTarefaInputValidator : AbstractValidator<ExcluirTarefaInput>
{
    public ExcluirTarefaInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("O identificador da tarefa é obrigatório.");
    }
}
