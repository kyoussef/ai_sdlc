using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Tasks.Dtos;
using TodoApp.Application.Tasks.Interfaces;
using TodoApp.Infrastructure.Data.Entities;

namespace TodoApp.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core-backed repository that encapsulates querying and persistence for <see cref="TaskItem"/> aggregates.
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Creates a new repository instance.
    /// </summary>
    /// <param name="db">Database context injected by the infrastructure layer.</param>
    public TaskRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<TaskDetailDto?> GetAsync(Guid id, CancellationToken ct)
    {
        var e = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : Map(e);
    }

    /// <inheritdoc />
    public async Task<TaskListResponse> ListAsync(TaskQuery q, CancellationToken ct)
    {
        var query = _db.Tasks.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var term = q.Q.Trim();
            query = query.Where(t => EF.Functions.Like(t.Title, $"%{term}%") || EF.Functions.Like(t.Description!, $"%{term}%"));
        }
        if (q.Priorities is { Count: > 0 })
        {
            var pVals = q.Priorities.Select(p => (Priority)p).ToList();
            query = query.Where(t => pVals.Contains(t.Priority));
        }
        var filterTags = (q.Tags is { Count: > 0 }) ? q.Tags!.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList() : null;

        if (q.Sort is not null)
        {
            var desc = q.Order == SortOrder.Desc;
            query = q.Sort switch
            {
                TaskSortBy.CreatedAt => desc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                TaskSortBy.DueDate => desc ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
                TaskSortBy.Priority => desc ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
                _ => query
            };
        }
        else
        {
            query = query.OrderByDescending(t => t.CreatedAt);
        }

        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);
        int total;
        List<TaskItem> items;

        if (filterTags is null)
        {
            total = await query.CountAsync(ct);
            items = await query.Skip(skip).Take(q.PageSize).ToListAsync(ct);
        }
        else
        {
            // EF Core + SQLite cannot translate List<string>.Contains over a JSON-converted column.
            // Apply tag filtering in-memory for MVP correctness (OR across selected tags).
            var all = await query.ToListAsync(ct);
            var tagSet = new HashSet<string>(filterTags!, StringComparer.OrdinalIgnoreCase);
            var filtered = all.Where(t => t.Tags != null && t.Tags.Any(tag => tagSet.Contains(tag))).ToList();
            total = filtered.Count;
            items = filtered.Skip(skip).Take(q.PageSize).ToList();
        }
        var dtoItems = items.Select(MapToDto).ToList();
        return new TaskListResponse(dtoItems, q.Page, q.PageSize, total);
    }

    /// <inheritdoc />
    public async Task<TaskDetailDto> CreateAsync(CreateTaskRequest r, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var e = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = r.Title.Trim(),
            Description = r.Description?.Trim(),
            DueDate = r.DueDate,
            Priority = (Priority)r.Priority,
            Tags = (r.Tags?.ToList() ?? new List<string>()).Take(10).ToList(),
            Completed = false,
            CreatedAt = now,
            UpdatedAt = now,
            RowVersion = Guid.NewGuid().ToByteArray(),
        };
        _db.Tasks.Add(e);
        await _db.SaveChangesAsync(ct);
        return Map(e);
    }

    /// <inheritdoc />
    public async Task<TaskDetailDto?> UpdateAsync(Guid id, UpdateTaskRequest r, CancellationToken ct)
    {
        var e = await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return null;
        if (r.RowVersion is not null) _db.Entry(e).OriginalValues[nameof(TaskItem.RowVersion)] = r.RowVersion;

        e.Title = r.Title.Trim();
        e.Description = r.Description?.Trim();
        e.DueDate = r.DueDate;
        e.Priority = (Priority)r.Priority;
        e.Tags = (r.Tags?.ToList() ?? new List<string>()).Take(10).ToList();
        e.Completed = r.Completed;
        e.UpdatedAt = DateTime.UtcNow;
        e.RowVersion = Guid.NewGuid().ToByteArray();

        await _db.SaveChangesAsync(ct);
        return Map(e);
    }

    /// <inheritdoc />
    public async Task<TaskDetailDto?> PatchAsync(Guid id, PatchTaskRequest r, CancellationToken ct)
    {
        var e = await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return null;
        if (r.RowVersion is not null) _db.Entry(e).OriginalValues[nameof(TaskItem.RowVersion)] = r.RowVersion;

        if (r.Title is not null) e.Title = r.Title.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.DueDate is not null) e.DueDate = r.DueDate;
        if (r.Priority is not null) e.Priority = (Priority)r.Priority.Value;
        if (r.Tags is not null) e.Tags = r.Tags.ToList();
        if (r.Completed is not null) e.Completed = r.Completed.Value;
        e.UpdatedAt = DateTime.UtcNow;
        e.RowVersion = Guid.NewGuid().ToByteArray();

        await _db.SaveChangesAsync(ct);
        return Map(e);
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct)
    {
        var e = await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;
        e.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static TaskDto MapToDto(TaskItem e) => new(
        e.Id,
        e.Title,
        e.Description,
        e.DueDate,
        (TaskPriority)e.Priority,
        e.Tags,
        e.Completed,
        e.CreatedAt,
        e.UpdatedAt,
        e.DeletedAt
    );

    private static TaskDetailDto Map(TaskItem e) => new(
        e.Id,
        e.Title,
        e.Description,
        e.DueDate,
        (TaskPriority)e.Priority,
        e.Tags,
        e.Completed,
        e.CreatedAt,
        e.UpdatedAt,
        e.DeletedAt
    );
}
