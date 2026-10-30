using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using TodoApp.Api.Features.Tarefas.AtualizarTarefa;
using TodoApp.Application.Features.Tarefas.CriarTarefa;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Api.OpenApi;

public sealed class TarefaSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.ParameterDescription?.Name == "status")
        {
            schema.Enum = [.. Enum.GetNames<StatusTarefa>().Select(valor => (JsonNode)valor)];
            schema.Examples = [(JsonNode)"Pendente"];
            schema.Description = "Filtra pelo status da tarefa (Pendente, EmAndamento ou Concluida), sem diferenciar maiúsculas de minúsculas.";
        }
        else if (context.JsonTypeInfo.Type == typeof(CriarTarefaInput) || context.JsonTypeInfo.Type == typeof(AtualizarTarefaRequest))
        {
            var vencimentoUtc = DateTime.UtcNow.Date.AddDays(7).AddHours(23).AddMinutes(59).AddSeconds(59);
            var vencimentoComOffset = new DateTimeOffset(vencimentoUtc).ToOffset(TimeSpan.FromHours(-3));

            schema.Examples =
            [
                new JsonObject
                {
                    ["titulo"] = "Pagar a conta de luz",
                    ["descricao"] = "Conta referente ao mês de outubro",
                    ["dataVencimento"] = vencimentoComOffset.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
                },
            ];
        }

        return Task.CompletedTask;
    }
}
