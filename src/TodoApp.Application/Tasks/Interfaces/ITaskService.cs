using System.IO;
using TodoApp.Application.Tasks.Dtos;

namespace TodoApp.Application.Tasks.Interfaces;

/// <summary>
/// Defines orchestration operations for task management exposed to the presentation layer.
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Retrieves a task by identifier.
    /// </summary>
    /// <param name="id">Unique task identifier.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>The task when found; otherwise <c>null</c>.</returns>
    Task<TaskDetailDto?> GetAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Executes a paged query over tasks using filtering and sorting settings.
    /// </summary>
    /// <param name="query">Filtering and paging settings.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>Paged list of tasks matching the criteria.</returns>
    Task<TaskListResponse> ListAsync(TaskQuery query, CancellationToken ct);

    /// <summary>
    /// Creates a task and returns the persisted representation and resource location.
    /// </summary>
    /// <param name="request">Payload describing the new task.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>The created task and relative URI clients can follow.</returns>
    Task<(TaskDetailDto task, Uri location)> CreateAsync(CreateTaskRequest request, CancellationToken ct);

    /// <summary>
    /// Replaces the full representation of an existing task.
    /// </summary>
    /// <param name="id">Identifier of the target task.</param>
    /// <param name="request">Complete payload including concurrency token.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>The updated task or <c>null</c> if it does not exist.</returns>
    Task<TaskDetailDto?> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct);

    /// <summary>
    /// Applies a partial update to an existing task.
    /// </summary>
    /// <param name="id">Identifier of the target task.</param>
    /// <param name="request">Patch payload with optional fields.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>The updated task or <c>null</c> if it does not exist.</returns>
    Task<TaskDetailDto?> PatchAsync(Guid id, PatchTaskRequest request, CancellationToken ct);

    /// <summary>
    /// Soft-deletes a task by recording its deletion timestamp.
    /// </summary>
    /// <param name="id">Identifier of the target task.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns><c>true</c> when a task was deleted; otherwise <c>false</c>.</returns>
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Imports tasks from a CSV stream, validating each row before persistence.
    /// </summary>
    /// <param name="csvStream">Readable stream containing CSV content.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>Summary detailing successful inserts and encountered errors.</returns>
    Task<TaskImportResult> ImportAsync(Stream csvStream, CancellationToken ct);
}