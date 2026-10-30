using FluentValidation;

namespace TodoApp.Application.Features.Tarefas.AtualizarTarefa;

public sealed class AtualizarTarefaInputValidator : AbstractValidator<AtualizarTarefaInput>
{
    public AtualizarTarefaInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("O identificador da tarefa é obrigatório.");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("O título da tarefa é obrigatório.")
            .MaximumLength(120).WithMessage("O título da tarefa deve ter no máximo 120 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(2000).WithMessage("A descrição da tarefa deve ter no máximo 2000 caracteres.");
    }
}
