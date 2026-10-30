using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TodoApp.Application.Abstractions;
using TodoApp.Application.Features.Tarefas.ExcluirTarefa;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Tests.Application.ExcluirTarefa;

public class ExcluirTarefaHandlerTests
{
    private static readonly DateTime Agora = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly ITarefaRepository _repository = Substitute.For<ITarefaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestLogger<ExcluirTarefaHandler> _logger = new();
    private readonly ExcluirTarefaHandler _handler;

    public ExcluirTarefaHandlerTests()
    {
        _handler = new ExcluirTarefaHandler(new ExcluirTarefaInputValidator(), _repository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_com_tarefa_existente_deve_remover_e_commitar()
    {
        var tarefa = Tarefa.Criar("Título", null, null, Agora).Value;
        _repository.ObterPorIdAsync(tarefa.Id, Arg.Any<CancellationToken>()).Returns(tarefa);

        var resultado = await _handler.Handle(new ExcluirTarefaInput(tarefa.Id), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        _repository.Received(1).Remover(tarefa);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_com_tarefa_inexistente_deve_retornar_falha_sem_commitar()
    {
        _repository.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Tarefa?)null);

        var resultado = await _handler.Handle(new ExcluirTarefaInput(Guid.NewGuid()), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.NotFound);
        _repository.DidNotReceiveWithAnyArgs().Remover(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_com_id_vazio_deve_retornar_falha_de_validacao_sem_consultar_o_repositorio()
    {
        var resultado = await _handler.Handle(new ExcluirTarefaInput(Guid.Empty), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.Validation);
        await _repository.DidNotReceiveWithAnyArgs().ObterPorIdAsync(default, default);
    }
}
