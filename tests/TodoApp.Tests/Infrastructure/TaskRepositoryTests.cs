using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Tasks.Dtos;
using TodoApp.Infrastructure.Data;
using TodoApp.Infrastructure.Data.Entities;
using TodoApp.Infrastructure.Data.Repositories;
using Xunit;

namespace TodoApp.Tests.Infrastructure;

public class TaskRepositoryTests : IAsyncLifetime
{
    private readonly SqliteConnection _conn;
    private readonly DbContextOptions<AppDbContext> _options;

    public TaskRepositoryTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
    }

    public async Task InitializeAsync()
    {
        await using var db = new AppDbContext(_options);
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync()
    {
        _conn.Dispose();
        return Task.CompletedTask;
    }

    private async Task SeedAsync(params TaskItem[] items)
    {
        await using var db = new AppDbContext(_options);
        db.Tasks.AddRange(items);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task ListAsync_Defaults_ReturnsSortedByCreatedDesc_WithPaging()
    {
        var now = DateTime.UtcNow;
        await SeedAsync(
            NewItem("A", now.AddMinutes(-10), Priority.Low),
            NewItem("B", now.AddMinutes(-5), Priority.Med),
            NewItem("C", now.AddMinutes(-1), Priority.High)
        );
        await using var db = new AppDbContext(_options);
        var repo = new TaskRepository(db);

        var query = new TaskQuery(1, 2, null, null, null, TaskSortBy.CreatedAt, SortOrder.Desc);
        var res = await repo.ListAsync(query, CancellationToken.None);

        res.Total.Should().Be(3);
        res.Items.Select(i => i.Title).Should().ContainInOrder("C", "B");
    }

    [Fact]
    public async Task ListAsync_Search_Q_FiltersByTitleOrDescription()
    {
        var now = DateTime.UtcNow;
        await SeedAsync(
            NewItem("Buy milk", now.AddMinutes(-3), Priority.Med, description: null),
            NewItem("Other", now.AddMinutes(-2), Priority.Med, description: "Pick up eggs"));

        await using var db = new AppDbContext(_options);
        var repo = new TaskRepository(db);
        var query = new TaskQuery(1, 10, "eggs", null, null, TaskSortBy.CreatedAt, SortOrder.Desc);
        var res = await repo.ListAsync(query, CancellationToken.None);

        res.Items.Should().ContainSingle(i => i.Description == "Pick up eggs");
    }

    [Fact]
    public async Task ListAsync_FilterByPriority_IncludesOnlySelected()
    {
        var now = DateTime.UtcNow;
        await SeedAsync(
            NewItem("Low", now.AddMinutes(-3), Priority.Low),
            NewItem("Med", now.AddMinutes(-2), Priority.Med),
            NewItem("High", now.AddMinutes(-1), Priority.High));

        await using var db = new AppDbContext(_options);
        var repo = new TaskRepository(db);
        var query = new TaskQuery(1, 10, null, new List<TaskPriority> { TaskPriority.Med, TaskPriority.High }, null, TaskSortBy.CreatedAt, SortOrder.Desc);
        var res = await repo.ListAsync(query, CancellationToken.None);

        res.Items.Select(x => x.Title).Should().BeEquivalentTo(new[] { "High", "Med" });
    }

    [Fact]
    public async Task ListAsync_FilterByTags_UsesInMemoryORLogic()
    {
        var now = DateTime.UtcNow;
        await SeedAsync(
            NewItem("T1", now.AddMinutes(-3), Priority.Med, tags: new() { "home" }),
            NewItem("T2", now.AddMinutes(-2), Priority.Med, tags: new() { "work" }),
            NewItem("T3", now.AddMinutes(-1), Priority.Med, tags: new() { "work", "urgent" }));

        await using var db = new AppDbContext(_options);
        var repo = new TaskRepository(db);
        var query = new TaskQuery(1, 10, null, null, new List<string> { "work", "urgent" }, TaskSortBy.CreatedAt, SortOrder.Desc);
        var res = await repo.ListAsync(query, CancellationToken.None);

        res.Items.Select(x => x.Title).Should().BeEquivalentTo(new[] { "T3", "T2" });
    }

    [Fact]
    public async Task ListAsync_SortByDueDate_Ascending()
    {
        var now = DateTime.UtcNow;
        await SeedAsync(
            NewItem("Later", now.AddMinutes(-3), Priority.Med, dueDate: DateOnly.FromDateTime(now.AddDays(2))),
            NewItem("Sooner", now.AddMinutes(-2), Priority.Med, dueDate: DateOnly.FromDateTime(now.AddDays(1)))
        );

        await using var db = new AppDbContext(_options);
        var repo = new TaskRepository(db);
        var query = new TaskQuery(1, 10, null, null, null, TaskSortBy.DueDate, SortOrder.Asc);
        var res = await repo.ListAsync(query, CancellationToken.None);

        res.Items.Select(x => x.Title).Should().ContainInOrder("Sooner", "Later");
    }

    [Fact]
    public async Task ListAsync_SortByPriority_Descending()
    {
        var now = DateTime.UtcNow;
        await SeedAsync(
            NewItem("Low", now.AddMinutes(-3), Priority.Low),
            NewItem("High", now.AddMinutes(-2), Priority.High)
        );

        await using var db = new AppDbContext(_options);
        var repo = new TaskRepository(db);
        var query = new TaskQuery(1, 10, null, null, null, TaskSortBy.Priority, SortOrder.Desc);
        var res = await repo.ListAsync(query, CancellationToken.None);

        res.Items.Select(x => x.Title).Should().ContainInOrder("High", "Low");
    }

    [Fact]
    public async Task CreateUpdatePatchDelete_Workflow_Works()
    {
        await using var db = new AppDbContext(_options);
        var repo = new TaskRepository(db);

        var create = new CreateTaskRequest("New", null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), TaskPriority.Low, new List<string> { "home" });
        var created = await repo.CreateAsync(create, CancellationToken.None);
        created.Id.Should().NotBeEmpty();

        var update = new UpdateTaskRequest("Updated", "desc", create.DueDate, TaskPriority.High, new List<string> { "urgent" }, true, null);
        var updated = await repo.UpdateAsync(created.Id, update, CancellationToken.None);
        updated!.Title.Should().Be("Updated");
        updated.Priority.Should().Be(TaskPriority.High);

        var patch = new PatchTaskRequest("Patched", null, null, null, null, null, null);
        var patched = await repo.PatchAsync(created.Id, patch, CancellationToken.None);
        patched!.Title.Should().Be("Patched");

        var deleted = await repo.SoftDeleteAsync(created.Id, CancellationToken.None);
        deleted.Should().BeTrue();

        var listed = await repo.ListAsync(new TaskQuery(1, 10, null, null, null, TaskSortBy.CreatedAt, SortOrder.Desc), CancellationToken.None);
        listed.Items.Should().NotContain(x => x.Id == created.Id);
    }

    private static TaskItem NewItem(string title, DateTime createdAt, Priority priority, string? description = null, List<string>? tags = null, DateOnly? dueDate = null, bool completed = false)
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
            Priority = priority,
            Tags = tags ?? new List<string>(),
            DueDate = dueDate,
            Completed = completed,
            RowVersion = Guid.NewGuid().ToByteArray()
        };
    }
}


