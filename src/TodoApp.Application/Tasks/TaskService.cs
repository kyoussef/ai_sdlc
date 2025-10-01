using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

    /// <inheritdoc />
    public async Task<TaskImportResult> ImportAsync(Stream csvStream, CancellationToken ct)
    {
        if (csvStream is null || !csvStream.CanRead) throw new ArgumentException("CSV stream is required", nameof(csvStream));
        if (csvStream.CanSeek) csvStream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(csvStream, Encoding.UTF8, leaveOpen: true);
        var errors = new List<string>();
        var createdIds = new List<Guid>();
        var lineNumber = 0;

        var headerLine = await reader.ReadLineAsync();
        lineNumber++;
        if (headerLine is null)
        {
            errors.Add("CSV file is empty.");
            return new TaskImportResult(0, errors.Count, errors, createdIds);
        }

        headerLine = headerLine.TrimEnd('\r');
        var headers = ParseCsvLine(headerLine);
        if (headers.Length == 0)
        {
            errors.Add("CSV header is invalid or contains mismatched quotes.");
            return new TaskImportResult(0, errors.Count, errors, createdIds);
        }

        int TitleIdx(string name) => Array.FindIndex(headers, h => h.Equals(name, StringComparison.OrdinalIgnoreCase));
        var titleIdx = TitleIdx("title");
        var descIdx = TitleIdx("description");
        var dueIdx = TitleIdx("dueDate");
        var priorityIdx = TitleIdx("priority");
        var tagsIdx = TitleIdx("tags");
        var completedIdx = TitleIdx("completed");

        if (titleIdx < 0 || dueIdx < 0 || priorityIdx < 0)
        {
            errors.Add("CSV header must contain at least 'title', 'dueDate', and 'priority'.");
            return new TaskImportResult(0, errors.Count, errors, createdIds);
        }

        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            ct.ThrowIfCancellationRequested();
            lineNumber++;
            line = line.TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line)) continue;

            var fields = ParseCsvLine(line);
            if (fields.Length == 0)
            {
                errors.Add($"Line {lineNumber}: invalid CSV format (mismatched quotes?).");
                continue;
            }
            if (fields.Length < headers.Length)
            {
                errors.Add($"Line {lineNumber}: expected {headers.Length} columns but found {fields.Length}.");
                continue;
            }

            string GetField(int idx) => idx >= 0 && idx < fields.Length ? fields[idx].Trim() : string.Empty;

            var title = GetField(titleIdx);
            var description = GetField(descIdx);
            var dueRaw = GetField(dueIdx);
            var priorityRaw = GetField(priorityIdx);
            var tagsRaw = GetField(tagsIdx);
            var completedRaw = GetField(completedIdx);

            if (string.IsNullOrWhiteSpace(title))
            {
                errors.Add($"Line {lineNumber}: title is required.");
                continue;
            }

            if (!TryParsePriority(priorityRaw, out var priority))
            {
                errors.Add($"Line {lineNumber}: invalid priority '{priorityRaw}'. Expected Low, Med, or High.");
                continue;
            }

            if (!TryParseDateOnly(dueRaw, out var dueDate))
            {
                errors.Add($"Line {lineNumber}: invalid dueDate '{dueRaw}'. Expected yyyy-MM-dd.");
                continue;
            }

            var tags = ParseTags(tagsRaw);
            var createRequest = new CreateTaskRequest(title, string.IsNullOrWhiteSpace(description) ? null : description, dueDate, priority, tags);

            bool markCompleted = TryParseBool(completedRaw, out var completedFlag) && completedFlag;

            try
            {
                ValidateCreate(createRequest);
                var created = await _repo.CreateAsync(createRequest, ct);
                createdIds.Add(created.Id);
                if (markCompleted)
                {
                    await _repo.PatchAsync(created.Id, new PatchTaskRequest(null, null, null, null, null, true, null), ct);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Line {lineNumber}: {ex.Message}");
            }
        }

        var successful = createdIds.Count;
        return new TaskImportResult(successful, errors.Count, errors, createdIds);
    }

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

    private static bool TryParsePriority(string value, out TaskPriority priority)
    {
        priority = TaskPriority.Med;
        if (string.IsNullOrWhiteSpace(value)) return false;

        switch (value.Trim().ToLowerInvariant())
        {
            case "low":
                priority = TaskPriority.Low;
                return true;
            case "med":
            case "medium":
                priority = TaskPriority.Med;
                return true;
            case "high":
                priority = TaskPriority.High;
                return true;
            default:
                return false;
        }
    }

    private static bool TryParseDateOnly(string value, out DateOnly dueDate)
    {
        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dueDate)) return true;
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
        {
            dueDate = DateOnly.FromDateTime(dt);
            return true;
        }
        return false;
    }

    private static bool TryParseBool(string value, out bool flag)
    {
        flag = false;
        if (string.IsNullOrWhiteSpace(value)) return false;

        if (bool.TryParse(value, out flag)) return true;

        if (value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            flag = true;
            return true;
        }

        return false;
    }

    private static IReadOnlyList<string>? ParseTags(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var tags = raw
            .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToList();
        return tags.Count == 0 ? null : tags;
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        if (inQuotes) return Array.Empty<string>();
        result.Add(current.ToString());
        return result.ToArray();
    }
}
