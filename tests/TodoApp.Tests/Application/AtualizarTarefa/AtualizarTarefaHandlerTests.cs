using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TodoApp.Application.Abstractions;
using TodoApp.Application.Features.Tarefas.AtualizarTarefa;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Tests.Application.AtualizarTarefa;

public class AtualizarTarefaHandlerTests
{
    private static readonly DateTime Agora = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly ITarefaRepository _repository = Substitute.For<ITarefaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly TestLogger<AtualizarTarefaHandler> _logger = new();
    private readonly AtualizarTarefaHandler _handler;

    public AtualizarTarefaHandlerTests()
    {
        _clock.UtcNow.Returns(Agora);
        _handler = new AtualizarTarefaHandler(new AtualizarTarefaInputValidator(), _repository, _unitOfWork, _clock, _logger);
    }

    [Fact]
    public async Task Handle_com_dados_validos_deve_atualizar_e_commitar()
    {
        var tarefa = Tarefa.Criar("Título antigo", null, null, Agora).Value;
        _repository.ObterPorIdAsync(tarefa.Id, Arg.Any<CancellationToken>()).Returns(tarefa);

        var input = new AtualizarTarefaInput(tarefa.Id, "Título novo", "Nova descrição", Agora.AddDays(2));

        var resultado = await _handler.Handle(input, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Titulo.Should().Be("Título novo");
        resultado.Value.Descricao.Should().Be("Nova descrição");
        resultado.Value.DataVencimento.Should().Be(Agora.AddDays(2));

        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_com_tarefa_inexistente_deve_retornar_falha_sem_commitar()
    {
        _repository.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Tarefa?)null);

        var resultado = await _handler.Handle(new AtualizarTarefaInput(Guid.NewGuid(), "Título", null, null), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.NotFound);

        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_com_titulo_invalido_deve_retornar_falha_sem_consultar_o_repositorio()
    {
        var resultado = await _handler.Handle(new AtualizarTarefaInput(Guid.NewGuid(), "", null, null), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.Validation);

        await _repository.DidNotReceiveWithAnyArgs().ObterPorIdAsync(default, default);
    }

    [Fact]
    public async Task Handle_com_vencimento_no_passado_deve_retornar_falha_de_dominio_sem_commitar()
    {
        var tarefa = Tarefa.Criar("Título", null, null, Agora).Value;
        _repository.ObterPorIdAsync(tarefa.Id, Arg.Any<CancellationToken>()).Returns(tarefa);

        var input = new AtualizarTarefaInput(tarefa.Id, "Título", null, Agora.AddDays(-1));

        var resultado = await _handler.Handle(input, CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be("Tarefa.DataVencimentoNoPassado");

        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}
