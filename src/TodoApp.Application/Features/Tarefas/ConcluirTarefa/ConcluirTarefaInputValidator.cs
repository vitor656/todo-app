using FluentValidation;

namespace TodoApp.Application.Features.Tarefas.ConcluirTarefa;

public sealed class ConcluirTarefaInputValidator : AbstractValidator<ConcluirTarefaInput>
{
    public ConcluirTarefaInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("O identificador da tarefa é obrigatório.");
    }
}
