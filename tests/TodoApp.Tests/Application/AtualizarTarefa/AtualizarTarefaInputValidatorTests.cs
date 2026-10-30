using FluentAssertions;
using TodoApp.Application.Features.Tarefas.AtualizarTarefa;

namespace TodoApp.Tests.Application.AtualizarTarefa;

public class AtualizarTarefaInputValidatorTests
{
    private readonly AtualizarTarefaInputValidator _validator = new();

    [Fact]
    public void Deve_ser_valido_com_dados_corretos()
    {
        var input = new AtualizarTarefaInput(Guid.NewGuid(), "Título válido", "Descrição", null);

        var resultado = _validator.Validate(input);

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Deve_ser_invalido_com_id_vazio()
    {
        var input = new AtualizarTarefaInput(Guid.Empty, "Título válido", null, null);

        var resultado = _validator.Validate(input);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == nameof(AtualizarTarefaInput.Id));
    }

    [Fact]
    public void Deve_ser_invalido_com_titulo_vazio()
    {
        var input = new AtualizarTarefaInput(Guid.NewGuid(), "", null, null);

        var resultado = _validator.Validate(input);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == nameof(AtualizarTarefaInput.Titulo));
    }

    [Fact]
    public void Deve_ser_invalido_com_descricao_muito_longa()
    {
        var input = new AtualizarTarefaInput(Guid.NewGuid(), "Título válido", new string('a', 2001), null);

        var resultado = _validator.Validate(input);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == nameof(AtualizarTarefaInput.Descricao));
    }
}
