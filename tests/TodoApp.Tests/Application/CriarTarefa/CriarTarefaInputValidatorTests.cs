using FluentAssertions;
using TodoApp.Application.Features.Tarefas.CriarTarefa;

namespace TodoApp.Tests.Application.CriarTarefa;

public class CriarTarefaInputValidatorTests
{
    private readonly CriarTarefaInputValidator _validator = new();

    [Fact]
    public void Deve_ser_valido_quando_titulo_e_descricao_respeitam_os_limites()
    {
        var input = new CriarTarefaInput("Título válido", "Descrição válida", null);

        var resultado = _validator.Validate(input);

        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_ser_invalido_quando_titulo_vazio(string titulo)
    {
        var input = new CriarTarefaInput(titulo, null, null);

        var resultado = _validator.Validate(input);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CriarTarefaInput.Titulo));
    }

    [Fact]
    public void Deve_ser_invalido_quando_titulo_excede_120_caracteres()
    {
        var input = new CriarTarefaInput(new string('a', 121), null, null);

        var resultado = _validator.Validate(input);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CriarTarefaInput.Titulo));
    }

    [Fact]
    public void Deve_ser_invalido_quando_descricao_excede_2000_caracteres()
    {
        var input = new CriarTarefaInput("Título", new string('a', 2001), null);

        var resultado = _validator.Validate(input);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CriarTarefaInput.Descricao));
    }
}
