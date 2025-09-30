using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.Tasks.Dtos;
using TodoApp.Application.Tasks.Interfaces;

namespace TodoApp.Api.Controllers;

/// <summary>
/// Exposes CRUD operations for tasks to API consumers, applying business rules from <see cref="ITaskService"/>.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    /// <summary>
    /// Initializes a new <see cref="TasksController"/> that delegates operations to the task service.
    /// </summary>
    /// <param name="service">Domain orchestration service that enforces validation and persistence rules.</param>
    public TasksController(ITaskService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves a paged task list filtered by search term, priority, tags, and sorted as requested.
    /// </summary>
    /// <param name="page">1-based page index; values less than 1 are coerced to 1.</param>
    /// <param name="pageSize">Number of items per page; constrained to 1-100 to protect the database.</param>
    /// <param name="q">Case-insensitive search term matched against title and description.</param>
    /// <param name="priority">Optional priority filters applied with OR semantics.</param>
    /// <param name="tag">Optional tag filters; blank values are ignored.</param>
    /// <param name="sort">Field used for ordering when provided.</param>
    /// <param name="order">Direction for the sort field; defaults to descending.</param>
    /// <param name="ct">Propagated cancellation token for the HTTP request.</param>
    /// <returns><see cref="TaskListResponse"/> payload with pagination metadata.</returns>
    /// <response code="200">Task list returned successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] TaskPriority[]? priority = null,
        [FromQuery] string[]? tag = null,
        [FromQuery] TaskSortBy? sort = TaskSortBy.CreatedAt,
        [FromQuery] SortOrder order = SortOrder.Desc,
        CancellationToken ct = default)
    {
        var priorities = priority is { Length: > 0 } ? priority!.ToList() : new List<TaskPriority>();
        var tags = tag is { Length: > 0 } ? tag!.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList() : new List<string>();
        var query = new TaskQuery(page < 1 ? 1 : page, Math.Clamp(pageSize, 1, 100), q, priorities, tags, sort, order);
        var result = await _service.ListAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Fetches a single task by identifier.
    /// </summary>
    /// <param name="id">Unique task identifier.</param>
    /// <param name="ct">Propagated cancellation token.</param>
    /// <returns>The requested task when found; otherwise <c>404 Not Found</c>.</returns>
    /// <response code="200">Task exists and is returned.</response>
    /// <response code="404">Task does not exist or was soft deleted.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDetailDto>> Get(Guid id, CancellationToken ct)
    {
        var task = await _service.GetAsync(id, ct);
        return task is null ? NotFound() : Ok(task);
    }

    /// <summary>
    /// Creates a new task and returns the created resource.
    /// </summary>
    /// <param name="request">Payload describing the task to create.</param>
    /// <param name="ct">Propagated cancellation token.</param>
    /// <returns>The persisted task along with its relative location.</returns>
    /// <remarks>Validation errors produce <c>400 Bad Request</c> via <see cref="ProblemDetailsMiddleware"/>.</remarks>
    /// <response code="201">Task created successfully.</response>
    /// <response code="400">Payload violates business validation rules.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDetailDto>> Create([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var (task, location) = await _service.CreateAsync(request, ct);
        return Created(location, task);
    }

    /// <summary>
    /// Replaces an existing task with the supplied payload.
    /// </summary>
    /// <param name="id">Identifier of the task to update.</param>
    /// <param name="request">Complete representation of the task including optimistic concurrency token.</param>
    /// <param name="ct">Propagated cancellation token.</param>
    /// <returns>The updated task when present; otherwise <c>404 Not Found</c>.</returns>
    /// <remarks>Concurrency conflicts yield <c>409 Conflict</c> via <see cref="ProblemDetailsMiddleware"/>.</remarks>
    /// <response code="200">Task updated successfully.</response>
    /// <response code="400">Payload fails validation.</response>
    /// <response code="404">Task not found for the supplied identifier.</response>
    /// <response code="409">Update rejected due to row version mismatch.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskDetailDto>> Replace(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Applies a partial update to a task using JSON merge semantics.
    /// </summary>
    /// <param name="id">Identifier of the task to patch.</param>
    /// <param name="request">Subset of fields to modify.</param>
    /// <param name="ct">Propagated cancellation token.</param>
    /// <returns>The updated task when present; otherwise <c>404 Not Found</c>.</returns>
    /// <remarks>Null fields are ignored; concurrency conflicts surface as <c>409 Conflict</c>.</remarks>
    /// <response code="200">Task patched successfully.</response>
    /// <response code="400">Patch payload fails validation or includes invalid ranges.</response>
    /// <response code="404">Task not found.</response>
    /// <response code="409">Patch rejected due to stale row version.</response>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskDetailDto>> Patch(Guid id, [FromBody] PatchTaskRequest request, CancellationToken ct)
    {
        var updated = await _service.PatchAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Bulk imports tasks from a CSV file.
    /// </summary>
    /// <param name="file">CSV file containing task rows.</param>
    /// <param name="ct">Propagated cancellation token.</param>
    /// <returns>Summary of imported tasks including errors.</returns>
    /// <response code="200">Import attempted and summary returned.</response>
    /// <response code="400">No file provided or file empty.</response>
    [HttpPost("import")]
    [ProducesResponseType(typeof(TaskImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskImportResult>> Import([FromForm] IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "A non-empty CSV file is required." });
        }

        await using var stream = file.OpenReadStream();
        var result = await _service.ImportAsync(stream, ct);
        return Ok(result);
    }

    /// <summary>
    /// Soft deletes a task by marking its deletion timestamp without removing the record.
    /// </summary>
    /// <param name="id">Identifier of the task to delete.</param>
    /// <param name="ct">Propagated cancellation token.</param>
    /// <returns><c>204 No Content</c> when the task was deleted; otherwise <c>404 Not Found</c>.</returns>
    /// <response code="204">Task marked as deleted.</response>
    /// <response code="404">Task not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _service.SoftDeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
