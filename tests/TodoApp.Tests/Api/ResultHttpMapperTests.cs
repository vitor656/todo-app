using FluentAssertions;
using Microsoft.AspNetCore.Http;
using TodoApp.Api.Contracts;
using TodoApp.Domain.Common;

namespace TodoApp.Tests.Api;

public class ResultHttpMapperTests
{
    [Fact]
    public void ToHttpResult_generico_com_sucesso_deve_retornar_200_com_envelope()
    {
        var httpContext = new DefaultHttpContext();
        var resultado = Result.Success(42);

        var httpResult = ResultHttpMapper.ToHttpResult(resultado, httpContext, "Sucesso.");

        GetStatusCode(httpResult).Should().Be(StatusCodes.Status200OK);

        var body = GetValue<ApiResponse<int>>(httpResult);
        body.Success.Should().BeTrue();
        body.Data.Should().Be(42);
        body.Message.Should().Be("Sucesso.");
        body.Errors.Should().BeEmpty();
        body.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToCreatedResult_com_sucesso_deve_retornar_201_com_envelope()
    {
        var httpContext = new DefaultHttpContext();
        var resultado = Result.Success(42);

        var httpResult = ResultHttpMapper.ToCreatedResult(resultado, httpContext, "Criado.", valor => $"/recursos/{valor}");

        GetStatusCode(httpResult).Should().Be(StatusCodes.Status201Created);

        var body = GetValue<ApiResponse<int>>(httpResult);
        body.Success.Should().BeTrue();
        body.Data.Should().Be(42);
    }

    [Fact]
    public void ToHttpResult_sem_dado_com_sucesso_deve_retornar_200_com_data_nulo()
    {
        var httpContext = new DefaultHttpContext();
        var resultado = Result.Success();

        var httpResult = ResultHttpMapper.ToHttpResult(resultado, httpContext, "Removido.");

        GetStatusCode(httpResult).Should().Be(StatusCodes.Status200OK);

        var body = GetValue<ApiResponse<object?>>(httpResult);
        body.Success.Should().BeTrue();
        body.Data.Should().BeNull();
    }

    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    public void ToHttpResult_com_falha_deve_mapear_status_http_correto(ErrorType tipo, int statusEsperado)
    {
        var httpContext = new DefaultHttpContext();
        var erro = CriarErro(tipo);
        var resultado = Result.Failure<int>(erro);

        var httpResult = ResultHttpMapper.ToHttpResult(resultado, httpContext, "Sucesso.");

        GetStatusCode(httpResult).Should().Be(statusEsperado);

        var body = GetValue<ApiResponse<object?>>(httpResult);
        body.Success.Should().BeFalse();
        body.Data.Should().BeNull();
        body.Message.Should().Be("Mensagem");
        body.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToHttpResult_com_falha_de_validacao_deve_incluir_erros_por_campo()
    {
        var httpContext = new DefaultHttpContext();
        var erro = Error.Validation("Codigo", "Mensagem", [new FieldError("Titulo", "Obrigatório")]);
        var resultado = Result.Failure<int>(erro);

        var httpResult = ResultHttpMapper.ToHttpResult(resultado, httpContext, "Sucesso.");

        var body = GetValue<ApiResponse<object?>>(httpResult);
        body.Errors.Should().ContainSingle(e => e.Field == "Titulo" && e.Message == "Obrigatório");
    }

    private static Error CriarErro(ErrorType tipo) => tipo switch
    {
        ErrorType.Validation => Error.Validation("Codigo", "Mensagem", [new FieldError("Campo", "Inválido")]),
        ErrorType.NotFound => Error.NotFound("Codigo", "Mensagem"),
        ErrorType.Conflict => Error.Conflict("Codigo", "Mensagem"),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
    };

    private static int GetStatusCode(IResult result) =>
        ((IStatusCodeHttpResult)result).StatusCode ?? throw new InvalidOperationException("Sem status code.");

    private static T GetValue<T>(IResult result) =>
        (T)((IValueHttpResult)result).Value!;
}
