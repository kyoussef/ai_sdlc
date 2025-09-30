using TodoApp.Application.Tasks.Dtos;

namespace TodoApp.Application.Tasks.Interfaces;

/// <summary>
/// Describes persistence operations for task aggregates.
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Retrieves a task by identifier using no-tracking semantics.
    /// </summary>
    /// <param name="id">Unique task identifier.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>The task when found; otherwise <c>null</c>.</returns>
    Task<TaskDetailDto?> GetAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Executes a paged and filtered query across tasks.
    /// </summary>
    /// <param name="query">Filtering, sorting, and paging parameters.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns><see cref="TaskListResponse"/> containing query results.</returns>
    Task<TaskListResponse> ListAsync(TaskQuery query, CancellationToken ct);

    /// <summary>
    /// Persists a new task using the supplied request payload.
    /// </summary>
    /// <param name="request">Validated task creation payload.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>The created task representation.</returns>
    Task<TaskDetailDto> CreateAsync(CreateTaskRequest request, CancellationToken ct);

    /// <summary>
    /// Applies a full update to an existing task.
    /// </summary>
    /// <param name="id">Identifier of the target task.</param>
    /// <param name="request">Full update payload including concurrency token when available.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>The updated task or <c>null</c> if no task matches the identifier.</returns>
    Task<TaskDetailDto?> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct);

    /// <summary>
    /// Applies partial updates to an existing task.
    /// </summary>
    /// <param name="id">Identifier of the target task.</param>
    /// <param name="request">Patch payload with optional fields.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>The updated task or <c>null</c> when not found.</returns>
    Task<TaskDetailDto?> PatchAsync(Guid id, PatchTaskRequest request, CancellationToken ct);

    /// <summary>
    /// Soft deletes a task by setting its deletion timestamp.
    /// </summary>
    /// <param name="id">Identifier of the task.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns><c>true</c> when the task existed and was deleted; otherwise <c>false</c>.</returns>
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct);
}
