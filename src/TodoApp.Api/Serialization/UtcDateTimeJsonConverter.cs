using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoApp.Api.Serialization;

/// <summary>
/// Normaliza qualquer <see cref="DateTime"/> recebido (com "Z", com offset ou sem nenhum dos dois)
/// para UTC antes de chegar ao domínio, já que a Infrastructure só aceita <see cref="DateTimeKind.Utc"/>
/// ao gravar em colunas "timestamp with time zone".
/// </summary>
public sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ParaUtc(reader.GetDateTime());

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(ParaUtc(value));

    /// <summary>
    /// Também usado fora da (de)serialização JSON para normalizar valores vindos de query string
    /// (que não passam por este converter), como em <c>ListarTarefasEndpoint</c>.
    /// </summary>
    public static DateTime ParaUtc(DateTime valor) => valor.Kind switch
    {
        DateTimeKind.Utc => valor,
        DateTimeKind.Local => valor.ToUniversalTime(),
        _ => DateTime.SpecifyKind(valor, DateTimeKind.Utc),
    };
}
