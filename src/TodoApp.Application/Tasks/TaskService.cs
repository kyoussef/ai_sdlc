using System;
using TodoApp.Application.Tasks.Dtos;
using TodoApp.Application.Tasks.Interfaces;

namespace TodoApp.Application.Tasks;

/// <summary>
/// Coordinates task-centric use cases and enforces business validation prior to persistence.
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;

    /// <summary>
    /// Initializes a new <see cref="TaskService"/>.
    /// </summary>
    /// <param name="repo">Repository providing persistence operations.</param>
    public TaskService(ITaskRepository repo)
    {
        _repo = repo;
    }

    /// <inheritdoc />
    public Task<TaskDetailDto?> GetAsync(Guid id, CancellationToken ct) => _repo.GetAsync(id, ct);

    /// <inheritdoc />
    public Task<TaskListResponse> ListAsync(TaskQuery query, CancellationToken ct) => _repo.ListAsync(query, ct);

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when the request violates create validation rules.</exception>
    public async Task<(TaskDetailDto task, Uri location)> CreateAsync(CreateTaskRequest request, CancellationToken ct)
    {
        ValidateCreate(request);
        var created = await _repo.CreateAsync(request, ct);
        var location = new Uri($"/api/tasks/{created.Id}", UriKind.Relative);
        return (created, location);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when the request violates update validation rules.</exception>
    public async Task<TaskDetailDto?> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct)
    {
        ValidateUpdate(request);
        return await _repo.UpdateAsync(id, request, ct);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when the request violates patch validation rules.</exception>
    public async Task<TaskDetailDto?> PatchAsync(Guid id, PatchTaskRequest request, CancellationToken ct)
    {
        ValidatePatch(request);
        return await _repo.PatchAsync(id, request, ct);
    }

    /// <inheritdoc />
    public Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct) => _repo.SoftDeleteAsync(id, ct);

    private static void ValidateCreate(CreateTaskRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Title)) throw new ArgumentException("Title is required", nameof(r.Title));
        if (r.Title.Length > 200) throw new ArgumentException("Title must be <= 200 characters", nameof(r.Title));
        if (r.Description is { Length: > 1000 }) throw new ArgumentException("Description must be <= 1000 characters", nameof(r.Description));
        if (r.DueDate is null) throw new ArgumentException("Due date is required", nameof(r.DueDate));
        if (r.Tags is { Count: > 10 }) throw new ArgumentException("Max 10 tags", nameof(r.Tags));
    }

    private static void ValidateUpdate(UpdateTaskRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Title)) throw new ArgumentException("Title is required", nameof(r.Title));
        if (r.Title.Length > 200) throw new ArgumentException("Title must be <= 200 characters", nameof(r.Title));
        if (r.Description is { Length: > 1000 }) throw new ArgumentException("Description must be <= 1000 characters", nameof(r.Description));
        if (r.Tags is { Count: > 10 }) throw new ArgumentException("Max 10 tags", nameof(r.Tags));
    }

    private static void ValidatePatch(PatchTaskRequest r)
    {
        if (r.Title is { Length: 0 } or { Length: > 200 }) throw new ArgumentException("Title must be 1-200 characters", nameof(r.Title));
        if (r.Description is { Length: > 1000 }) throw new ArgumentException("Description must be <= 1000 characters", nameof(r.Description));
        if (r.Tags is { Count: > 10 }) throw new ArgumentException("Max 10 tags", nameof(r.Tags));
    }
}
