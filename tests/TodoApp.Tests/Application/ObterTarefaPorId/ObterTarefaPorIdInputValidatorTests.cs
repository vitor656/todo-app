using FluentAssertions;
using TodoApp.Application.Features.Tarefas.ObterTarefaPorId;

namespace TodoApp.Tests.Application.ObterTarefaPorId;

public class ObterTarefaPorIdInputValidatorTests
{
    private readonly ObterTarefaPorIdInputValidator _validator = new();

    [Fact]
    public void Deve_ser_valido_quando_id_nao_e_vazio()
    {
        var resultado = _validator.Validate(new ObterTarefaPorIdInput(Guid.NewGuid()));

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Deve_ser_invalido_quando_id_e_vazio()
    {
        var resultado = _validator.Validate(new ObterTarefaPorIdInput(Guid.Empty));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(ObterTarefaPorIdInput.Id));
    }
}
