using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TodoApp.Application.Abstractions;
using TodoApp.Application.Features.Tarefas.ObterTarefaPorId;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Tests.Application.ObterTarefaPorId;

public class ObterTarefaPorIdHandlerTests
{
    private static readonly DateTime Agora = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly ITarefaRepository _repository = Substitute.For<ITarefaRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly TestLogger<ObterTarefaPorIdHandler> _logger = new();
    private readonly ObterTarefaPorIdHandler _handler;

    public ObterTarefaPorIdHandlerTests()
    {
        _clock.UtcNow.Returns(Agora);
        _handler = new ObterTarefaPorIdHandler(new ObterTarefaPorIdInputValidator(), _repository, _clock, _logger);
    }

    [Fact]
    public async Task Handle_com_tarefa_existente_deve_retornar_sucesso()
    {
        var tarefa = Tarefa.Criar("Título", null, null, Agora).Value;
        _repository.ObterPorIdAsync(tarefa.Id, Arg.Any<CancellationToken>()).Returns(tarefa);

        var resultado = await _handler.Handle(new ObterTarefaPorIdInput(tarefa.Id), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Id.Should().Be(tarefa.Id);
    }

    [Fact]
    public async Task Handle_com_tarefa_inexistente_deve_retornar_falha_de_nao_encontrada()
    {
        var id = Guid.NewGuid();
        _repository.ObterPorIdAsync(id, Arg.Any<CancellationToken>()).Returns((Tarefa?)null);

        var resultado = await _handler.Handle(new ObterTarefaPorIdInput(id), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.NotFound);
        resultado.Error!.Code.Should().Be("Tarefa.NaoEncontrada");
    }

    [Fact]
    public async Task Handle_com_id_vazio_deve_retornar_falha_de_validacao_sem_consultar_o_repositorio()
    {
        var resultado = await _handler.Handle(new ObterTarefaPorIdInput(Guid.Empty), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.Validation);

        await _repository.DidNotReceiveWithAnyArgs().ObterPorIdAsync(default, default);
    }
}
