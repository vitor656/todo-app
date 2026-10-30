using FluentAssertions;
using TodoApp.Application.Features.Tarefas.ListarTarefas;

namespace TodoApp.Tests.Application.ListarTarefas;

public class ListarTarefasInputValidatorTests
{
    private readonly ListarTarefasInputValidator _validator = new();

    [Fact]
    public void Deve_ser_valido_sem_nenhum_filtro()
    {
        var resultado = _validator.Validate(new ListarTarefasInput(null, null, null));

        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Pendente")]
    [InlineData("pendente")]
    [InlineData("EMANDAMENTO")]
    [InlineData("Concluida")]
    public void Deve_ser_valido_com_status_em_qualquer_caixa(string status)
    {
        var resultado = _validator.Validate(new ListarTarefasInput(status, null, null));

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Deve_ser_invalido_com_status_desconhecido()
    {
        var resultado = _validator.Validate(new ListarTarefasInput("Invalido", null, null));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(ListarTarefasInput.Status));
    }

    [Fact]
    public void Deve_ser_invalido_quando_vencimento_de_e_posterior_ao_vencimento_ate()
    {
        var agora = DateTime.UtcNow;

        var resultado = _validator.Validate(new ListarTarefasInput(null, agora.AddDays(5), agora));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(ListarTarefasInput.VencimentoDe));
    }

    [Fact]
    public void Deve_ser_valido_quando_vencimento_de_e_igual_ao_vencimento_ate()
    {
        var agora = DateTime.UtcNow;

        var resultado = _validator.Validate(new ListarTarefasInput(null, agora, agora));

        resultado.IsValid.Should().BeTrue();
    }
}
