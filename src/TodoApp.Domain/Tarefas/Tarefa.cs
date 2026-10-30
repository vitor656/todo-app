using TodoApp.Domain.Common;

namespace TodoApp.Domain.Tarefas;

public sealed class Tarefa
{
    private Tarefa()
    {
    }

    private Tarefa(Guid id, string titulo, string? descricao, DateTime? dataVencimento, DateTime criadoEm)
    {
        Id = id;
        Titulo = titulo;
        Descricao = descricao;
        Status = StatusTarefa.Pendente;
        DataVencimento = dataVencimento;
        CriadoEm = criadoEm;
    }

    public Guid Id { get; private set; }

    public string Titulo { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    public StatusTarefa Status { get; private set; }

    public DateTime? DataVencimento { get; private set; }

    public DateTime CriadoEm { get; private set; }

    public static Result<Tarefa> Criar(string titulo, string? descricao, DateTime? dataVencimento, DateTime agora)
    {
        var erro = ValidarTitulo(titulo) ?? ValidarDescricao(descricao) ?? ValidarVencimento(dataVencimento, agora);
        if (erro is not null)
        {
            return Result.Failure<Tarefa>(erro);
        }

        var tarefa = new Tarefa(Guid.NewGuid(), titulo.Trim(), descricao, dataVencimento, agora);
        return Result.Success(tarefa);
    }

    public Result Iniciar()
    {
        if (Status != StatusTarefa.Pendente)
        {
            return Result.Failure(TarefaErros.TransicaoInvalida(Status, "iniciar"));
        }

        Status = StatusTarefa.EmAndamento;
        return Result.Success();
    }

    public Result Concluir()
    {
        if (Status is not (StatusTarefa.Pendente or StatusTarefa.EmAndamento))
        {
            return Result.Failure(TarefaErros.TransicaoInvalida(Status, "concluir"));
        }

        Status = StatusTarefa.Concluida;
        return Result.Success();
    }

    public Result Reabrir()
    {
        if (Status != StatusTarefa.Concluida)
        {
            return Result.Failure(TarefaErros.TransicaoInvalida(Status, "reabrir"));
        }

        Status = StatusTarefa.Pendente;
        return Result.Success();
    }

    public Result AtualizarDetalhes(string titulo, string? descricao)
    {
        var erro = ValidarTitulo(titulo) ?? ValidarDescricao(descricao);
        if (erro is not null)
        {
            return Result.Failure(erro);
        }

        Titulo = titulo.Trim();
        Descricao = descricao;
        return Result.Success();
    }

    public Result Reagendar(DateTime? dataVencimento, DateTime agora)
    {
        if (dataVencimento is not null && dataVencimento != DataVencimento)
        {
            var erro = ValidarVencimento(dataVencimento, agora);
            if (erro is not null)
            {
                return Result.Failure(erro);
            }
        }

        DataVencimento = dataVencimento;
        return Result.Success();
    }

    public bool EstaVencida(DateTime agora) =>
        DataVencimento is not null && DataVencimento < agora && Status != StatusTarefa.Concluida;

    private static Error? ValidarTitulo(string titulo)
    {
        var tituloAparado = titulo.Trim();

        if (tituloAparado.Length == 0)
        {
            return TarefaErros.TituloObrigatorio();
        }

        if (tituloAparado.Length > 120)
        {
            return TarefaErros.TituloMuitoLongo();
        }

        return null;
    }

    private static Error? ValidarDescricao(string? descricao) =>
        descricao is not null && descricao.Length > 2000
            ? TarefaErros.DescricaoMuitoLonga()
            : null;

    private static Error? ValidarVencimento(DateTime? dataVencimento, DateTime agora) =>
        dataVencimento is not null && dataVencimento < agora
            ? TarefaErros.DataVencimentoNoPassado()
            : null;
}
