using FluentValidation;

namespace TodoApp.Application.Features.Tarefas.CriarTarefa;

public sealed class CriarTarefaInputValidator : AbstractValidator<CriarTarefaInput>
{
    public CriarTarefaInputValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("O título da tarefa é obrigatório.")
            .MaximumLength(120).WithMessage("O título da tarefa deve ter no máximo 120 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(2000).WithMessage("A descrição da tarefa deve ter no máximo 2000 caracteres.");
    }
}
