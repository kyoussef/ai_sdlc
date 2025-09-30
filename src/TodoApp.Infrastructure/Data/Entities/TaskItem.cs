using System;
using System.Collections.Generic;

namespace TodoApp.Infrastructure.Data.Entities;

/// <summary>
/// Persistence-layer priority enumeration mirroring <see cref="TodoApp.Application.Tasks.Dtos.TaskPriority"/>.
/// </summary>
public enum Priority
{
    /// <summary>Work items with minimal urgency.</summary>
    Low,
    /// <summary>Default priority balancing work and personal tasks.</summary>
    Med,
    /// <summary>Requires immediate attention.</summary>
    High
}

/// <summary>
/// Database entity backing the task aggregate, including concurrency metadata.
/// </summary>
public class TaskItem
{
    /// <summary>
    /// Primary key generated as a GUID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Required short title (<= 200 characters).
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional long-form description (<= 1000 characters).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional due date stored as date-only value.
    /// </summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// Priority classification persisted as an integer.
    /// </summary>
    public Priority Priority { get; set; } = Priority.Med;

    /// <summary>
    /// Up to ten tags serialized to JSON for SQLite compatibility.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Indicates whether the task is complete.
    /// </summary>
    public bool Completed { get; set; }

    /// <summary>
    /// Creation timestamp captured in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last-updated timestamp captured in UTC.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Soft-delete marker. Null when the task remains active.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Optimistic concurrency token updated on each mutation.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
