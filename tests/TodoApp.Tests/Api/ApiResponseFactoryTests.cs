using FluentAssertions;
using Microsoft.AspNetCore.Http;
using TodoApp.Api.Contracts;

namespace TodoApp.Tests.Api;

public class ApiResponseFactoryTests
{
    [Fact]
    public void Success_deve_preencher_todos_os_campos_com_success_verdadeiro()
    {
        var resposta = ApiResponseFactory.Success(42, "Operação concluída.", "trace-123");

        resposta.Success.Should().BeTrue();
        resposta.Message.Should().Be("Operação concluída.");
        resposta.Data.Should().Be(42);
        resposta.Errors.Should().BeEmpty();
        resposta.TraceId.Should().Be("trace-123");
    }

    [Fact]
    public void Failure_deve_preencher_todos_os_campos_com_success_falso_e_data_nulo()
    {
        var erros = new[] { new ApiError("Titulo", "Obrigatório") };

        var resposta = ApiResponseFactory.Failure<int>("Falha de validação.", erros, "trace-456");

        resposta.Success.Should().BeFalse();
        resposta.Message.Should().Be("Falha de validação.");
        resposta.Data.Should().Be(0);
        resposta.Errors.Should().BeEquivalentTo(erros);
        resposta.TraceId.Should().Be("trace-456");
    }

    [Fact]
    public void Failure_com_tipo_referencia_deve_ter_data_nulo()
    {
        var resposta = ApiResponseFactory.Failure<string>("Falha.", [], "trace-789");

        resposta.Data.Should().BeNull();
    }

    [Fact]
    public void GetTraceId_sem_activity_corrente_deve_usar_o_trace_identifier_do_http_context()
    {
        var httpContext = new DefaultHttpContext { TraceIdentifier = "trace-identifier-xyz" };

        var traceId = ApiResponseFactory.GetTraceId(httpContext);

        traceId.Should().Be("trace-identifier-xyz");
    }
}
