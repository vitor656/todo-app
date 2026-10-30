using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoApp.Api.ErrorHandling;

namespace TodoApp.Tests.Api;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_com_excecao_desconhecida_deve_retornar_500_generico_sem_detalhes()
    {
        var logger = new TestLogger<GlobalExceptionHandler>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = CriarHttpContext();

        var tratado = await handler.TryHandleAsync(httpContext, new InvalidOperationException("Detalhe interno sensível"), CancellationToken.None);

        tratado.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var corpo = await LerCorpo(httpContext);
        corpo.GetProperty("success").GetBoolean().Should().BeFalse();
        corpo.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
        corpo.GetProperty("traceId").GetString().Should().NotBeNullOrEmpty();

        var mensagem = corpo.GetProperty("message").GetString();
        mensagem.Should().Be("Ocorreu um erro interno. Informe o traceId ao suporte.");
        mensagem.Should().NotContain("Detalhe interno sensível");

        logger.Chamadas.Should().ContainSingle(c => c.Level == LogLevel.Error);
    }

    [Fact]
    public async Task TryHandleAsync_com_bad_http_request_exception_deve_retornar_400()
    {
        var logger = new TestLogger<GlobalExceptionHandler>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = CriarHttpContext();

        var tratado = await handler.TryHandleAsync(httpContext, new BadHttpRequestException("corpo malformado"), CancellationToken.None);

        tratado.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        logger.Chamadas.Should().ContainSingle(c => c.Level == LogLevel.Warning);
        logger.Chamadas.Should().NotContain(c => c.Level == LogLevel.Error);
    }

    [Fact]
    public async Task TryHandleAsync_com_db_update_concurrency_exception_deve_retornar_409()
    {
        var logger = new TestLogger<GlobalExceptionHandler>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = CriarHttpContext();

        var tratado = await handler.TryHandleAsync(httpContext, new DbUpdateConcurrencyException("conflito"), CancellationToken.None);

        tratado.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        logger.Chamadas.Should().ContainSingle(c => c.Level == LogLevel.Warning);
        logger.Chamadas.Should().NotContain(c => c.Level == LogLevel.Error);
    }

    [Fact]
    public async Task TryHandleAsync_com_cancelamento_pelo_cliente_nao_deve_gerar_corpo_de_erro_nem_log()
    {
        var logger = new TestLogger<GlobalExceptionHandler>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = CriarHttpContext();

        var tratado = await handler.TryHandleAsync(httpContext, new OperationCanceledException(), CancellationToken.None);

        tratado.Should().BeTrue();
        httpContext.Response.Body.Length.Should().Be(0);
        logger.Chamadas.Should().BeEmpty();
    }

    private static DefaultHttpContext CriarHttpContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider(),
        };
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    private static async Task<JsonElement> LerCorpo(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;
        using var documento = await JsonDocument.ParseAsync(httpContext.Response.Body);
        return documento.RootElement.Clone();
    }
}
