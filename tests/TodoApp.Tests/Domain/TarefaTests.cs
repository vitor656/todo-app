using FluentAssertions;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Tests.Domain;

public class TarefaTests
{
    private static readonly DateTime Agora = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Criar_com_dados_validos_deve_retornar_sucesso()
    {
        var resultado = Tarefa.Criar("Pagar a conta de luz", "Descrição qualquer", Agora.AddDays(1), Agora);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Titulo.Should().Be("Pagar a conta de luz");
        resultado.Value.Descricao.Should().Be("Descrição qualquer");
        resultado.Value.Status.Should().Be(StatusTarefa.Pendente);
        resultado.Value.DataVencimento.Should().Be(Agora.AddDays(1));
        resultado.Value.CriadoEm.Should().Be(Agora);
        resultado.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Criar_deve_aparar_o_titulo_antes_de_validar()
    {
        var resultado = Tarefa.Criar("   Pagar a conta de luz   ", null, null, Agora);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Titulo.Should().Be("Pagar a conta de luz");
    }

    [Fact]
    public void Criar_sem_descricao_e_sem_vencimento_deve_retornar_sucesso()
    {
        var resultado = Tarefa.Criar("Tarefa simples", null, null, Agora);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Descricao.Should().BeNull();
        resultado.Value.DataVencimento.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_com_titulo_vazio_deve_retornar_falha(string titulo)
    {
        var resultado = Tarefa.Criar(titulo, null, null, Agora);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be("Tarefa.TituloObrigatorio");
        resultado.Error!.Type.Should().Be(ErrorType.Validation);
        resultado.Error!.FieldErrors.Should().ContainSingle(e => e.Field == "Titulo");
    }

    [Fact]
    public void Criar_com_titulo_de_120_caracteres_deve_retornar_sucesso()
    {
        var titulo = new string('a', 120);

        var resultado = Tarefa.Criar(titulo, null, null, Agora);

        resultado.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Criar_com_titulo_de_121_caracteres_deve_retornar_falha()
    {
        var titulo = new string('a', 121);

        var resultado = Tarefa.Criar(titulo, null, null, Agora);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be("Tarefa.TituloMuitoLongo");
    }

    [Fact]
    public void Criar_com_descricao_de_2000_caracteres_deve_retornar_sucesso()
    {
        var descricao = new string('a', 2000);

        var resultado = Tarefa.Criar("Título", descricao, null, Agora);

        resultado.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Criar_com_descricao_de_2001_caracteres_deve_retornar_falha()
    {
        var descricao = new string('a', 2001);

        var resultado = Tarefa.Criar("Título", descricao, null, Agora);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be("Tarefa.DescricaoMuitoLonga");
    }

    [Fact]
    public void Criar_com_vencimento_no_passado_deve_retornar_falha()
    {
        var resultado = Tarefa.Criar("Título", null, Agora.AddDays(-1), Agora);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be("Tarefa.DataVencimentoNoPassado");
    }

    [Fact]
    public void Criar_com_vencimento_igual_a_agora_deve_retornar_sucesso()
    {
        var resultado = Tarefa.Criar("Título", null, Agora, Agora);

        resultado.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Iniciar_a_partir_de_pendente_deve_mudar_status_para_em_andamento()
    {
        var tarefa = CriarTarefaValida();

        var resultado = tarefa.Iniciar();

        resultado.IsSuccess.Should().BeTrue();
        tarefa.Status.Should().Be(StatusTarefa.EmAndamento);
    }

    [Theory]
    [InlineData(StatusTarefa.EmAndamento)]
    [InlineData(StatusTarefa.Concluida)]
    public void Iniciar_a_partir_de_status_diferente_de_pendente_deve_retornar_falha(StatusTarefa statusInicial)
    {
        var tarefa = CriarTarefaComStatus(statusInicial);

        var resultado = tarefa.Iniciar();

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.Conflict);
        resultado.Error!.Code.Should().Be("Tarefa.TransicaoInvalida");
        tarefa.Status.Should().Be(statusInicial);
    }

    [Theory]
    [InlineData(StatusTarefa.Pendente)]
    [InlineData(StatusTarefa.EmAndamento)]
    public void Concluir_a_partir_de_pendente_ou_em_andamento_deve_mudar_status_para_concluida(StatusTarefa statusInicial)
    {
        var tarefa = CriarTarefaComStatus(statusInicial);

        var resultado = tarefa.Concluir();

        resultado.IsSuccess.Should().BeTrue();
        tarefa.Status.Should().Be(StatusTarefa.Concluida);
    }

    [Fact]
    public void Concluir_a_partir_de_concluida_deve_retornar_falha()
    {
        var tarefa = CriarTarefaComStatus(StatusTarefa.Concluida);

        var resultado = tarefa.Concluir();

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.Conflict);
        tarefa.Status.Should().Be(StatusTarefa.Concluida);
    }

    [Fact]
    public void Reabrir_a_partir_de_concluida_deve_mudar_status_para_pendente()
    {
        var tarefa = CriarTarefaComStatus(StatusTarefa.Concluida);

        var resultado = tarefa.Reabrir();

        resultado.IsSuccess.Should().BeTrue();
        tarefa.Status.Should().Be(StatusTarefa.Pendente);
    }

    [Theory]
    [InlineData(StatusTarefa.Pendente)]
    [InlineData(StatusTarefa.EmAndamento)]
    public void Reabrir_a_partir_de_status_diferente_de_concluida_deve_retornar_falha(StatusTarefa statusInicial)
    {
        var tarefa = CriarTarefaComStatus(statusInicial);

        var resultado = tarefa.Reabrir();

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.Conflict);
        tarefa.Status.Should().Be(statusInicial);
    }

    [Fact]
    public void AtualizarDetalhes_com_dados_validos_deve_alterar_titulo_e_descricao_sem_alterar_status()
    {
        var tarefa = CriarTarefaValida();
        tarefa.Iniciar();

        var resultado = tarefa.AtualizarDetalhes("Novo título", "Nova descrição");

        resultado.IsSuccess.Should().BeTrue();
        tarefa.Titulo.Should().Be("Novo título");
        tarefa.Descricao.Should().Be("Nova descrição");
        tarefa.Status.Should().Be(StatusTarefa.EmAndamento);
    }

    [Fact]
    public void AtualizarDetalhes_com_titulo_invalido_deve_retornar_falha_e_nao_alterar_nada()
    {
        var tarefa = CriarTarefaValida();

        var resultado = tarefa.AtualizarDetalhes("", "Nova descrição");

        resultado.IsFailure.Should().BeTrue();
        tarefa.Titulo.Should().Be("Título válido");
        tarefa.Descricao.Should().BeNull();
    }

    [Fact]
    public void Reagendar_com_data_futura_deve_alterar_a_data_de_vencimento()
    {
        var tarefa = CriarTarefaValida();
        var novaData = Agora.AddDays(5);

        var resultado = tarefa.Reagendar(novaData, Agora);

        resultado.IsSuccess.Should().BeTrue();
        tarefa.DataVencimento.Should().Be(novaData);
    }

    [Fact]
    public void Reagendar_com_data_no_passado_deve_retornar_falha()
    {
        var tarefa = CriarTarefaValida();

        var resultado = tarefa.Reagendar(Agora.AddDays(-1), Agora);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be("Tarefa.DataVencimentoNoPassado");
        tarefa.DataVencimento.Should().BeNull();
    }

    [Fact]
    public void Reagendar_reenviando_a_mesma_data_no_passado_deve_retornar_sucesso()
    {
        var tarefa = Tarefa.Criar("Título", null, Agora.AddDays(1), Agora).Value;
        tarefa.Reagendar(Agora.AddDays(1), Agora);

        var resultado = tarefa.Reagendar(Agora.AddDays(1), Agora.AddDays(2));

        resultado.IsSuccess.Should().BeTrue();
        tarefa.DataVencimento.Should().Be(Agora.AddDays(1));
    }

    [Fact]
    public void Reagendar_para_nulo_deve_remover_a_data_de_vencimento()
    {
        var tarefa = CriarTarefaValida(Agora.AddDays(1));

        var resultado = tarefa.Reagendar(null, Agora);

        resultado.IsSuccess.Should().BeTrue();
        tarefa.DataVencimento.Should().BeNull();
    }

    [Fact]
    public void Reagendar_nunca_altera_o_status()
    {
        var tarefa = CriarTarefaValida();
        tarefa.Iniciar();

        tarefa.Reagendar(Agora.AddDays(3), Agora);

        tarefa.Status.Should().Be(StatusTarefa.EmAndamento);
    }

    [Fact]
    public void EstaVencida_deve_ser_verdadeira_quando_vencimento_no_passado_e_nao_concluida()
    {
        var tarefa = CriarTarefaValida(Agora.AddDays(1));

        tarefa.EstaVencida(Agora.AddDays(2)).Should().BeTrue();
    }

    [Fact]
    public void EstaVencida_deve_ser_falsa_quando_concluida_mesmo_com_vencimento_no_passado()
    {
        var tarefa = CriarTarefaValida(Agora.AddDays(1));
        tarefa.Concluir();

        tarefa.EstaVencida(Agora.AddDays(2)).Should().BeFalse();
    }

    [Fact]
    public void EstaVencida_deve_ser_falsa_quando_nao_ha_vencimento()
    {
        var tarefa = CriarTarefaValida(null);

        tarefa.EstaVencida(Agora.AddDays(2)).Should().BeFalse();
    }

    [Fact]
    public void EstaVencida_deve_ser_falsa_quando_vencimento_no_futuro()
    {
        var tarefa = CriarTarefaValida(Agora.AddDays(5));

        tarefa.EstaVencida(Agora).Should().BeFalse();
    }

    private static Tarefa CriarTarefaValida(DateTime? dataVencimento = null) =>
        Tarefa.Criar("Título válido", null, dataVencimento, Agora).Value;

    private static Tarefa CriarTarefaComStatus(StatusTarefa status)
    {
        var tarefa = CriarTarefaValida();

        switch (status)
        {
            case StatusTarefa.Pendente:
                break;
            case StatusTarefa.EmAndamento:
                tarefa.Iniciar();
                break;
            case StatusTarefa.Concluida:
                tarefa.Concluir();
                break;
        }

        return tarefa;
    }
}
