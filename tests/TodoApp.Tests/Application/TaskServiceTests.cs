using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TodoApp.Application.Tasks;
using TodoApp.Application.Tasks.Dtos;
using TodoApp.Application.Tasks.Interfaces;
using Xunit;

namespace TodoApp.Tests.Application;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repo = new();

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedWithLocation()
    {
        var req = new CreateTaskRequest("Buy milk", null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), TaskPriority.Med, null);
        var created = new TaskDetailDto(Guid.NewGuid(), req.Title, null, req.DueDate, req.Priority, Array.Empty<string>(), false, DateTime.UtcNow, DateTime.UtcNow, null);
        _repo.Setup(r => r.CreateAsync(req, It.IsAny<CancellationToken>())).ReturnsAsync(created);
        var sut = new TaskService(_repo.Object);

        var (task, location) = await sut.CreateAsync(req, CancellationToken.None);

        task.Should().BeEquivalentTo(created);
        location.ToString().Should().Be($"/api/tasks/{created.Id}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_InvalidTitle_Throws(string? title)
    {
        var req = new CreateTaskRequest(title!, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), TaskPriority.Low, null);
        var sut = new TaskService(_repo.Object);
        Func<Task> act = () => sut.CreateAsync(req, CancellationToken.None);        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Title is required*");
    }

    [Fact]
    public async Task CreateAsync_MissingDueDate_Throws()
    {
        var req = new CreateTaskRequest("Title", null, null, TaskPriority.Low, null);
        var sut = new TaskService(_repo.Object);
        Func<Task> act = () => sut.CreateAsync(req, CancellationToken.None);        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Due date is required*");
    }

    [Fact]
    public async Task UpdateAsync_TitleTooLong_Throws()
    {
        var title = new string('a', 201);
        var req = new UpdateTaskRequest(title, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), TaskPriority.High, null, false, null);
        var sut = new TaskService(_repo.Object);
        Func<Task> act = () => sut.UpdateAsync(Guid.NewGuid(), req, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*<= 200*");
    }

    [Fact]
    public async Task PatchAsync_DescriptionTooLong_Throws()
    {
        var desc = new string('x', 1001);
        var req = new PatchTaskRequest(null, desc, null, null, null, null, null);
        var sut = new TaskService(_repo.Object);
        Func<Task> act = () => sut.PatchAsync(Guid.NewGuid(), req, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*<= 1000*");
    }
}









