using System;
using System.Collections.Generic;

namespace TodoApp.Application.Tasks.Dtos;

/// <summary>
/// Supported priority levels surfaced to both API and UI clients.
/// </summary>
public enum TaskPriority
{
    /// <summary>Work items with minimal urgency.</summary>
    Low,
    /// <summary>Default priority balancing work and personal tasks.</summary>
    Med,
    /// <summary>Requires immediate attention.</summary>
    High
}

/// <summary>
/// Lightweight projection used to render task summaries in list views.
/// </summary>
/// <param name="Id">Persistent identifier for the task.</param>
/// <param name="Title">Short human-readable title (<= 200 characters).</param>
/// <param name="Description">Optional rich description limited to 1000 characters.</param>
/// <param name="DueDate">Optional date-only deadline; null when unscheduled.</param>
/// <param name="Priority">Task urgency classification.</param>
/// <param name="Tags">Tag collection capped at 10 items, case-insensitive comparisons.</param>
/// <param name="Completed">Indicates whether the task is marked done.</param>
/// <param name="CreatedAt">UTC timestamp when the task was created.</param>
/// <param name="UpdatedAt">UTC timestamp when the task last changed.</param>
/// <param name="DeletedAt">Soft-delete marker; null when active.</param>
public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskPriority Priority,
    IReadOnlyList<string> Tags,
    bool Completed,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt
);

/// <summary>
/// Full representation of a task returned for detail views and modifications.
/// </summary>
/// <param name="Id">Persistent identifier for the task.</param>
/// <param name="Title">Short human-readable title (<= 200 characters).</param>
/// <param name="Description">Optional rich description limited to 1000 characters.</param>
/// <param name="DueDate">Optional date-only deadline; null when unscheduled.</param>
/// <param name="Priority">Task urgency classification.</param>
/// <param name="Tags">Tag collection capped at 10 items, case-insensitive comparisons.</param>
/// <param name="Completed">Indicates whether the task is marked done.</param>
/// <param name="CreatedAt">UTC timestamp when the task was created.</param>
/// <param name="UpdatedAt">UTC timestamp when the task last changed.</param>
/// <param name="DeletedAt">Soft-delete marker; null when active.</param>
public record TaskDetailDto(
    Guid Id,
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskPriority Priority,
    IReadOnlyList<string> Tags,
    bool Completed,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt
);

/// <summary>
/// Request payload used to create a new task.
/// </summary>
/// <param name="Title">Required title; trimmed and limited to 200 characters.</param>
/// <param name="Description">Optional description up to 1000 characters.</param>
/// <param name="DueDate">Required due date for newly created items.</param>
/// <param name="Priority">Priority classification for the task.</param>
/// <param name="Tags">Optional tag list limited to 10 entries.</param>
public record CreateTaskRequest(
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskPriority Priority,
    IReadOnlyList<string>? Tags
);

/// <summary>
/// Request payload for full updates where all fields must be provided.
/// </summary>
/// <param name="Title">Required title; trimmed and limited to 200 characters.</param>
/// <param name="Description">Optional description up to 1000 characters.</param>
/// <param name="DueDate">Optional due date; null clears the deadline.</param>
/// <param name="Priority">Required priority classification.</param>
/// <param name="Tags">Optional tag list limited to 10 entries.</param>
/// <param name="Completed">Completion state of the task.</param>
/// <param name="RowVersion">Optimistic concurrency token supplied by clients.</param>
public record UpdateTaskRequest(
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskPriority Priority,
    IReadOnlyList<string>? Tags,
    bool Completed,
    byte[]? RowVersion
);

/// <summary>
/// Request payload for partial updates, where null fields are ignored.
/// </summary>
/// <param name="Title">Optional new title; when provided must be 1-200 characters.</param>
/// <param name="Description">Optional new description up to 1000 characters.</param>
/// <param name="DueDate">Optional due date override.</param>
/// <param name="Priority">Optional priority update.</param>
/// <param name="Tags">Optional replacement tag list (max 10 entries).</param>
/// <param name="Completed">Optional completion flag.</param>
/// <param name="RowVersion">Optimistic concurrency token supplied by clients.</param>
public record PatchTaskRequest(
    string? Title,
    string? Description,
    DateOnly? DueDate,
    TaskPriority? Priority,
    IReadOnlyList<string>? Tags,
    bool? Completed,
    byte[]? RowVersion
);

/// <summary>
/// Paginated response shape for list endpoints.
/// </summary>
/// <param name="Items">Page of task summaries.</param>
/// <param name="Page">Current 1-based page index.</param>
/// <param name="PageSize">Maximum number of items returned.</param>
/// <param name="Total">Total matching records before paging.</param>
public record TaskListResponse(
    IReadOnlyList<TaskDto> Items,
    int Page,
    int PageSize,
    int Total
);

/// <summary>
/// Supported sort fields for list queries.
/// </summary>
public enum TaskSortBy
{
    /// <summary>Orders tasks by creation timestamp.</summary>
    CreatedAt,
    /// <summary>Orders tasks by due date with nulls last when ascending.</summary>
    DueDate,
    /// <summary>Orders tasks by priority value.</summary>
    Priority
}

/// <summary>
/// Sort order direction semantics.
/// </summary>
public enum SortOrder
{
    /// <summary>Ascending ordering.</summary>
    Asc,
    /// <summary>Descending ordering.</summary>
    Desc
}

/// <summary>
/// Container for list query parameters used by repository filtering logic.
/// </summary>
/// <param name="Page">1-based page index; negative values should be normalized.</param>
/// <param name="PageSize">Number of items per page; repository clamps the value.</param>
/// <param name="Q">Optional search term matched against title and description.</param>
/// <param name="Priorities">Optional priority filters; OR semantics.</param>
/// <param name="Tags">Optional tag filters; trimmed before evaluation.</param>
/// <param name="Sort">Field used for ordering when provided.</param>
/// <param name="Order">Sort direction when <paramref name="Sort"/> is specified.</param>
public record TaskQuery(
    int Page,
    int PageSize,
    string? Q,
    IReadOnlyList<TaskPriority>? Priorities,
    IReadOnlyList<string>? Tags,
    TaskSortBy? Sort,
    SortOrder Order
);
