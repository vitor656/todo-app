using System.Text.Json;
using FluentAssertions;
using TodoApp.Api.Serialization;

namespace TodoApp.Tests.Api;

public class UtcDateTimeJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new UtcDateTimeJsonConverter() },
    };

    [Fact]
    public void Deserializar_com_Z_deve_manter_o_instante_em_utc()
    {
        var resultado = JsonSerializer.Deserialize<DateTime>("\"2026-11-01T12:00:00Z\"", _options);

        resultado.Kind.Should().Be(DateTimeKind.Utc);
        resultado.Should().Be(new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Deserializar_com_offset_deve_converter_para_o_instante_equivalente_em_utc()
    {
        var resultado = JsonSerializer.Deserialize<DateTime>("\"2026-11-01T09:00:00-03:00\"", _options);

        resultado.Kind.Should().Be(DateTimeKind.Utc);
        resultado.Should().Be(new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Deserializar_sem_z_nem_offset_deve_tratar_o_valor_literal_como_utc()
    {
        var resultado = JsonSerializer.Deserialize<DateTime>("\"2026-11-01T12:00:00\"", _options);

        resultado.Kind.Should().Be(DateTimeKind.Utc);
        resultado.Should().Be(new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Serializar_deve_escrever_sufixo_Z()
    {
        var json = JsonSerializer.Serialize(new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc), _options);

        json.Should().Be("\"2026-11-01T12:00:00Z\"");
    }

    [Fact]
    public void Deserializar_data_nullable_ausente_deve_retornar_nulo()
    {
        var resultado = JsonSerializer.Deserialize<DateTime?>("null", _options);

        resultado.Should().BeNull();
    }
}
