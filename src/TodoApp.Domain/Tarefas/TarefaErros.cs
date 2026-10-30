using TodoApp.Domain.Common;

namespace TodoApp.Domain.Tarefas;

public static class TarefaErros
{
    public static Error TituloObrigatorio() =>
        Error.Validation(
            "Tarefa.TituloObrigatorio",
            "O título da tarefa é obrigatório.",
            [new FieldError("Titulo", "O título da tarefa é obrigatório.")]);

    public static Error TituloMuitoLongo() =>
        Error.Validation(
            "Tarefa.TituloMuitoLongo",
            "O título da tarefa deve ter no máximo 120 caracteres.",
            [new FieldError("Titulo", "O título da tarefa deve ter no máximo 120 caracteres.")]);

    public static Error DescricaoMuitoLonga() =>
        Error.Validation(
            "Tarefa.DescricaoMuitoLonga",
            "A descrição da tarefa deve ter no máximo 2000 caracteres.",
            [new FieldError("Descricao", "A descrição da tarefa deve ter no máximo 2000 caracteres.")]);

    public static Error DataVencimentoNoPassado() =>
        Error.Validation(
            "Tarefa.DataVencimentoNoPassado",
            "A data de vencimento não pode estar no passado.",
            [new FieldError("DataVencimento", "A data de vencimento não pode estar no passado.")]);

    public static Error NaoEncontrada() =>
        Error.NotFound("Tarefa.NaoEncontrada", "Tarefa não encontrada.");

    public static Error TransicaoInvalida(StatusTarefa statusAtual, string acao) =>
        Error.Conflict(
            "Tarefa.TransicaoInvalida",
            $"Não é possível {acao} a tarefa porque o status atual é {Rotulo(statusAtual)}.");

    private static string Rotulo(StatusTarefa status) => status switch
    {
        StatusTarefa.Pendente => "Pendente",
        StatusTarefa.EmAndamento => "Em Andamento",
        StatusTarefa.Concluida => "Concluída",
        _ => status.ToString(),
    };
}
