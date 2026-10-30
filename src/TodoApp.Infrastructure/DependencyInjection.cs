using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TodoApp.Application.Abstractions;
using TodoApp.Infrastructure.Persistence;
using TodoApp.Infrastructure.Time;

namespace TodoApp.Infrastructure;

public static partial class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'ConnectionStrings:Default' não foi configurada.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));

        services.AddScoped<ITarefaRepository, TarefaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IClock, SystemClock>();

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (!configuration.GetValue<bool>("Database:MigrateOnStartup"))
        {
            LogMigracoesDesativadas(logger);
            return;
        }

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();

        if (pendingMigrations.Count > 0)
        {
            LogAplicandoMigrations(logger, pendingMigrations.Count);
            await context.Database.MigrateAsync();
            LogBancoAtualizado(logger);
        }
    }

    [LoggerMessage(EventId = 9101, Level = LogLevel.Information, Message = "Aplicando {Quantidade} migration(s) pendente(s)")]
    private static partial void LogAplicandoMigrations(ILogger logger, int quantidade);

    [LoggerMessage(EventId = 9102, Level = LogLevel.Information, Message = "Banco de dados atualizado")]
    private static partial void LogBancoAtualizado(ILogger logger);

    [LoggerMessage(EventId = 9103, Level = LogLevel.Debug, Message = "Migrations automáticas desativadas neste ambiente")]
    private static partial void LogMigracoesDesativadas(ILogger logger);
}
