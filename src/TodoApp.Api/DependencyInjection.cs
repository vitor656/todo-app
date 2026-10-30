using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.SwaggerUI;
using TodoApp.Api.Contracts;
using TodoApp.Api.Endpoints;
using TodoApp.Api.ErrorHandling;
using TodoApp.Application;

namespace TodoApp.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [typeof(ApplicationAssemblyMarker)];
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddOpenApi();

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        foreach (var endpointType in typeof(DependencyInjection).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t)))
        {
            services.AddSingleton(typeof(IEndpoint), endpointType);
        }

        return services;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages(async statusCodeContext =>
        {
            var httpContext = statusCodeContext.HttpContext;
            var traceId = ApiResponseFactory.GetTraceId(httpContext);
            var message = httpContext.Response.StatusCode switch
            {
                StatusCodes.Status404NotFound => "Recurso não encontrado.",
                StatusCodes.Status405MethodNotAllowed => "Método não permitido para este recurso.",
                StatusCodes.Status415UnsupportedMediaType => "Tipo de conteúdo não suportado.",
                _ => "Ocorreu um erro ao processar a solicitação.",
            };

            var response = ApiResponseFactory.Failure<object?>(message, [], traceId);
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(response);
        });

        if (app.Configuration.GetValue<bool>("Swagger:Habilitado"))
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "TodoApp API v1");
                options.RoutePrefix = "swagger";
            });

            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
        }

        var group = app.MapGroup("/api/v1");

        foreach (var endpoint in app.Services.GetServices<IEndpoint>())
        {
            endpoint.MapEndpoint(group);
        }

        return app;
    }
}
