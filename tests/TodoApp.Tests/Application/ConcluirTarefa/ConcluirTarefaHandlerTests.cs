using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TodoApp.Application.Abstractions;
using TodoApp.Application.Features.Tarefas.ConcluirTarefa;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Tests.Application.ConcluirTarefa;

public class ConcluirTarefaHandlerTests
{
    private static readonly DateTime Agora = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly ITarefaRepository _repository = Substitute.For<ITarefaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly TestLogger<ConcluirTarefaHandler> _logger = new();
    private readonly ConcluirTarefaHandler _handler;

    public ConcluirTarefaHandlerTests()
    {
        _clock.UtcNow.Returns(Agora);
        _handler = new ConcluirTarefaHandler(new ConcluirTarefaInputValidator(), _repository, _unitOfWork, _clock, _logger);
    }

    [Theory]
    [InlineData(StatusTarefa.Pendente)]
    [InlineData(StatusTarefa.EmAndamento)]
    public async Task Handle_com_status_valido_deve_concluir_e_commitar(StatusTarefa statusInicial)
    {
        var tarefa = Tarefa.Criar("Título", null, null, Agora).Value;
        if (statusInicial == StatusTarefa.EmAndamento)
        {
            tarefa.Iniciar();
        }

        _repository.ObterPorIdAsync(tarefa.Id, Arg.Any<CancellationToken>()).Returns(tarefa);

        var resultado = await _handler.Handle(new ConcluirTarefaInput(tarefa.Id), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Status.Should().Be(StatusTarefa.Concluida);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_com_tarefa_inexistente_deve_retornar_falha_sem_commitar()
    {
        _repository.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Tarefa?)null);

        var resultado = await _handler.Handle(new ConcluirTarefaInput(Guid.NewGuid()), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.NotFound);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_com_tarefa_ja_concluida_deve_retornar_conflito_sem_commitar()
    {
        var tarefa = Tarefa.Criar("Título", null, null, Agora).Value;
        tarefa.Concluir();
        _repository.ObterPorIdAsync(tarefa.Id, Arg.Any<CancellationToken>()).Returns(tarefa);

        var resultado = await _handler.Handle(new ConcluirTarefaInput(tarefa.Id), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.Conflict);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}
