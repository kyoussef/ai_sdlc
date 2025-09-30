using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoApp.Infrastructure.Data.Entities;

namespace TodoApp.Infrastructure.Data;

/// <summary>
/// Populates the SQLite database with deterministic seed data for demos and tests.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Seeds baseline task data when the database is empty.
    /// </summary>
    /// <param name="db">Database context.</param>
    /// <param name="logger">Optional logger for telemetry.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    public static async Task SeedAsync(AppDbContext db, ILogger? logger = null, CancellationToken ct = default)
    {
        if (await db.Tasks.AnyAsync(ct)) return;

        var now = DateTime.UtcNow;
        var items = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Title = "Buy milk", Priority = Priority.Med, Tags = new(){"home"}, Completed = false, CreatedAt = now.AddMinutes(-30), UpdatedAt = now.AddMinutes(-30), RowVersion = Guid.NewGuid().ToByteArray() },
            new() { Id = Guid.NewGuid(), Title = "Write weekly report", Description = "Summarize progress and blockers", Priority = Priority.High, Tags = new(){"work"}, Completed = false, CreatedAt = now.AddHours(-3), UpdatedAt = now.AddHours(-3), DueDate = DateOnly.FromDateTime(now.AddDays(1)), RowVersion = Guid.NewGuid().ToByteArray() },
            new() { Id = Guid.NewGuid(), Title = "Pick up dry cleaning", Priority = Priority.Low, Tags = new(){"errands"}, Completed = false, CreatedAt = now.AddHours(-12), UpdatedAt = now.AddHours(-12), RowVersion = Guid.NewGuid().ToByteArray() },
            new() { Id = Guid.NewGuid(), Title = "Plan sprint backlog", Priority = Priority.Med, Tags = new(){"work","planning"}, Completed = true, CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1), RowVersion = Guid.NewGuid().ToByteArray() },
            new() { Id = Guid.NewGuid(), Title = "Book dentist appointment", Priority = Priority.Med, Tags = new(){"health"}, Completed = false, CreatedAt = now.AddMinutes(-10), UpdatedAt = now.AddMinutes(-10), DueDate = DateOnly.FromDateTime(now.AddDays(14)), RowVersion = Guid.NewGuid().ToByteArray() },
        };

        db.Tasks.AddRange(items);
        await db.SaveChangesAsync(ct);
        logger?.LogInformation("Seeded {Count} tasks", items.Count);
    }
}
