using FluentAssertions;
using TodoApp.Domain.Common;

namespace TodoApp.Tests.Domain;

public class ResultTests
{
    [Fact]
    public void Success_deve_criar_resultado_sem_erro()
    {
        var resultado = Result.Success();

        resultado.IsSuccess.Should().BeTrue();
        resultado.IsFailure.Should().BeFalse();
        resultado.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_deve_criar_resultado_com_erro()
    {
        var erro = Error.NotFound("Codigo", "Mensagem");

        var resultado = Result.Failure(erro);

        resultado.IsSuccess.Should().BeFalse();
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(erro);
    }

    [Fact]
    public void SuccessGenerico_deve_expor_o_valor()
    {
        var resultado = Result.Success(42);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(42);
    }

    [Fact]
    public void FailureGenerico_ao_acessar_o_valor_deve_lancar_excecao()
    {
        var resultado = Result.Failure<int>(Error.NotFound("Codigo", "Mensagem"));

        var acao = () => resultado.Value;

        acao.Should().Throw<InvalidOperationException>();
    }
}
