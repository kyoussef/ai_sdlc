using Microsoft.Extensions.DependencyInjection;
using TodoApp.Application.Tasks;
using TodoApp.Application.Tasks.Interfaces;

namespace TodoApp.Api.Startup;

/// <summary>
/// Registers presentation-layer services and wiring for the API project.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required by the API layer including controllers and task service facade.
    /// </summary>
    /// <param name="services">Service collection to extend.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}
