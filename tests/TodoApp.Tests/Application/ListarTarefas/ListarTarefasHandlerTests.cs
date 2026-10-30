using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TodoApp.Application.Abstractions;
using TodoApp.Application.Features.Tarefas.ListarTarefas;
using TodoApp.Domain.Common;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Tests.Application.ListarTarefas;

public class ListarTarefasHandlerTests
{
    private static readonly DateTime Agora = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly ITarefaRepository _repository = Substitute.For<ITarefaRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly TestLogger<ListarTarefasHandler> _logger = new();
    private readonly ListarTarefasHandler _handler;

    public ListarTarefasHandlerTests()
    {
        _clock.UtcNow.Returns(Agora);
        _handler = new ListarTarefasHandler(new ListarTarefasInputValidator(), _repository, _clock, _logger);
    }

    [Fact]
    public async Task Handle_sem_filtros_deve_retornar_todas_as_tarefas_mapeadas()
    {
        var tarefa1 = Tarefa.Criar("Primeira", null, null, Agora).Value;
        var tarefa2 = Tarefa.Criar("Segunda", null, null, Agora).Value;
        _repository.ListarAsync(null, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<Tarefa> { tarefa1, tarefa2 });

        var resultado = await _handler.Handle(new ListarTarefasInput(null, null, null), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().HaveCount(2);
        resultado.Value.Select(o => o.Id).Should().BeEquivalentTo([tarefa1.Id, tarefa2.Id]);
    }

    [Fact]
    public async Task Handle_sem_resultados_deve_retornar_sucesso_com_lista_vazia()
    {
        _repository.ListarAsync(null, null, null, Arg.Any<CancellationToken>()).Returns(new List<Tarefa>());

        var resultado = await _handler.Handle(new ListarTarefasInput(null, null, null), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_com_status_deve_converter_para_enum_e_repassar_ao_repositorio()
    {
        _repository.ListarAsync(StatusTarefa.EmAndamento, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<Tarefa>());

        var resultado = await _handler.Handle(new ListarTarefasInput("emandamento", null, null), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        await _repository.Received(1).ListarAsync(StatusTarefa.EmAndamento, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_com_filtros_invalidos_deve_retornar_falha_sem_consultar_o_repositorio()
    {
        var resultado = await _handler.Handle(new ListarTarefasInput("Invalido", null, null), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Type.Should().Be(ErrorType.Validation);

        await _repository.DidNotReceiveWithAnyArgs().ListarAsync(default, default, default, default);
    }
}
