using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Application.Tasks.Interfaces;
using TodoApp.Infrastructure.Data;
using TodoApp.Infrastructure.Data.Repositories;

namespace TodoApp.Infrastructure.Startup;

/// <summary>
/// Provides dependency injection helpers for the infrastructure layer.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers persistence dependencies such as <see cref="AppDbContext"/> and repositories.
    /// </summary>
    /// <param name="services">Service collection to extend.</param>
    /// <param name="connectionString">Optional SQLite connection string; when empty uses in-memory provider defaults.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseSqlite(connectionString);
            }
        });

        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}
