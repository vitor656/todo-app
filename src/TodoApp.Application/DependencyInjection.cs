using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TodoApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(ApplicationAssemblyMarker.Assembly);

        return services;
    }
}
