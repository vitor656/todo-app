using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TodoApp.Application.Abstractions;
using TodoApp.Application.Features.Tarefas.ReabrirTarefa;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Tests.Application.ReabrirTarefa;

public class ReabrirTarefaHandlerTests
{
    private static readonly DateTime Agora = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly ITarefaRepository _repository = Substitute.For<ITarefaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly TestLogger<ReabrirTarefaHandler> _logger = new();
    private readonly ReabrirTarefaHandler _handler;

    public ReabrirTarefaHandlerTests()
    {
        _clock.UtcNow.Returns(Agora);
        _handler = new ReabrirTarefaHandler(new ReabrirTarefaInputValidator(), _repository, _unitOfWork, _clock, _logger);
    }

    [Fact]
    public async Task Handle_com_tarefa_concluida_deve_reabrir_e_commitar()
    {
        var tarefa = Tarefa.Criar("Título", null, null, Agora).Value;
        tarefa.Concluir();
        _repository.ObterPorIdAsync(tarefa.Id, Arg.Any<CancellationToken>()).Returns(tarefa);

        var resultado = await _handler.Handle(new ReabrirTarefaInput(tarefa.Id), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Status.Should().Be(StatusTarefa.Pendente);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_com_tarefa_inexistente_deve_retornar_falha_sem_commitar()
    {
        _repository.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Tarefa?)null);

        var resultado = await _handler.Handle(new ReabrirTarefaInput(Guid.NewGuid()), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.NotFound);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_com_tarefa_pendente_deve_retornar_conflito_sem_commitar()
    {
        var tarefa = Tarefa.Criar("Título", null, null, Agora).Value;
        _repository.ObterPorIdAsync(tarefa.Id, Arg.Any<CancellationToken>()).Returns(tarefa);

        var resultado = await _handler.Handle(new ReabrirTarefaInput(tarefa.Id), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.Conflict);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}
