using FluentValidation;

namespace TodoApp.Application.Features.Tarefas.ReabrirTarefa;

public sealed class ReabrirTarefaInputValidator : AbstractValidator<ReabrirTarefaInput>
{
    public ReabrirTarefaInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("O identificador da tarefa é obrigatório.");
    }
}
