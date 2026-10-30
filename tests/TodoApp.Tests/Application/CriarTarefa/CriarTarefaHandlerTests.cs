using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TodoApp.Application.Abstractions;
using TodoApp.Application.Features.Tarefas.CriarTarefa;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Tests.Application.CriarTarefa;

public class CriarTarefaHandlerTests
{
    private static readonly DateTime Agora = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly ITarefaRepository _repository = Substitute.For<ITarefaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly TestLogger<CriarTarefaHandler> _logger = new();
    private readonly CriarTarefaHandler _handler;

    public CriarTarefaHandlerTests()
    {
        _clock.UtcNow.Returns(Agora);
        _handler = new CriarTarefaHandler(new CriarTarefaInputValidator(), _repository, _unitOfWork, _clock, _logger);
    }

    [Fact]
    public async Task Handle_com_dados_validos_deve_adicionar_a_tarefa_e_commitar()
    {
        var input = new CriarTarefaInput("Pagar a conta de luz", "Descrição", Agora.AddDays(1));

        var resultado = await _handler.Handle(input, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Titulo.Should().Be("Pagar a conta de luz");
        resultado.Value.Status.Should().Be(StatusTarefa.Pendente);

        _repository.Received(1).Adicionar(Arg.Is<Tarefa>(t => t.Titulo == "Pagar a conta de luz"));
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_com_titulo_invalido_deve_retornar_falha_de_validacao_sem_tocar_no_repositorio()
    {
        var input = new CriarTarefaInput("", null, null);

        var resultado = await _handler.Handle(input, CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.Validation);
        resultado.Error!.FieldErrors.Should().ContainSingle(e => e.Field == "Titulo");

        _repository.DidNotReceiveWithAnyArgs().Adicionar(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_com_vencimento_no_passado_deve_retornar_falha_de_dominio_sem_commitar()
    {
        var input = new CriarTarefaInput("Título válido", null, Agora.AddDays(-1));

        var resultado = await _handler.Handle(input, CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be("Tarefa.DataVencimentoNoPassado");

        _repository.DidNotReceiveWithAnyArgs().Adicionar(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}
