using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoApp.Api.Controllers;
using TodoApp.Application.Tasks.Dtos;
using TodoApp.Application.Tasks.Interfaces;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace TodoApp.Tests.Api;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _svc = new();

    [Fact]
    public async Task List_ReturnsOkWithPayload()
    {
        var expected = new TaskListResponse(new List<TaskDto>(), 1, 20, 0);
        _svc.Setup(s => s.ListAsync(It.IsAny<TaskQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var ctl = new TasksController(_svc.Object);

        var result = await ctl.List();
        result.Result.Should().BeOfType<OkObjectResult>();
        (result.Result as OkObjectResult)!.Value.Should().Be(expected);
    }

    [Fact]
    public async Task Create_ReturnsCreatedWithLocation()
    {
        var create = new CreateTaskRequest("Title", null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), TaskPriority.Low, null);
        var dto = new TaskDetailDto(Guid.NewGuid(), create.Title, null, create.DueDate, create.Priority, Array.Empty<string>(), false, DateTime.UtcNow, DateTime.UtcNow, null);
        var location = new Uri($"/api/tasks/{dto.Id}", UriKind.Relative);
        _svc.Setup(s => s.CreateAsync(create, It.IsAny<CancellationToken>())).ReturnsAsync((dto, location));
        var ctl = new TasksController(_svc.Object);

        var result = await ctl.Create(create, CancellationToken.None);
        var created = result.Result as CreatedResult;
        created.Should().NotBeNull();
        created!.Location.Should().Be(location.ToString());
        created.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Get_NotFound_Returns404()
    {
        _svc.Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TaskDetailDto?)null);
        var ctl = new TasksController(_svc.Object);
        var r = await ctl.Get(Guid.NewGuid(), CancellationToken.None);
        r.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Get_Found_ReturnsOk()
    {
        var dto = new TaskDetailDto(Guid.NewGuid(), "T", null, null, TaskPriority.Low, Array.Empty<string>(), false, DateTime.UtcNow, DateTime.UtcNow, null);
        _svc.Setup(s => s.GetAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var ctl = new TasksController(_svc.Object);
        var r = await ctl.Get(dto.Id, CancellationToken.None);
        (r.Result as OkObjectResult)!.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Replace_Ok_Returns200()
    {
        var id = Guid.NewGuid();
        var req = new UpdateTaskRequest("T", null, null, TaskPriority.Med, null, false, null);
        var updated = new TaskDetailDto(id, req.Title, null, null, req.Priority, Array.Empty<string>(), req.Completed, DateTime.UtcNow, DateTime.UtcNow, null);
        _svc.Setup(s => s.UpdateAsync(id, req, It.IsAny<CancellationToken>())).ReturnsAsync(updated);
        var ctl = new TasksController(_svc.Object);
        var r = await ctl.Replace(id, req, CancellationToken.None);
        (r.Result as OkObjectResult)!.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Replace_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        var req = new UpdateTaskRequest("T", null, null, TaskPriority.Med, null, false, null);
        _svc.Setup(s => s.UpdateAsync(id, req, It.IsAny<CancellationToken>())).ReturnsAsync((TaskDetailDto?)null);
        var ctl = new TasksController(_svc.Object);
        var r = await ctl.Replace(id, req, CancellationToken.None);
        r.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Patch_Ok_Returns200()
    {
        var id = Guid.NewGuid();
        var req = new PatchTaskRequest("New", null, null, null, null, null, null);
        var dto = new TaskDetailDto(id, req.Title!, null, null, TaskPriority.Low, Array.Empty<string>(), false, DateTime.UtcNow, DateTime.UtcNow, null);
        _svc.Setup(s => s.PatchAsync(id, req, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var ctl = new TasksController(_svc.Object);
        var r = await ctl.Patch(id, req, CancellationToken.None);
        (r.Result as OkObjectResult)!.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Patch_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        var req = new PatchTaskRequest("X", null, null, null, null, null, null);
        _svc.Setup(s => s.PatchAsync(id, req, It.IsAny<CancellationToken>())).ReturnsAsync((TaskDetailDto?)null);
        var ctl = new TasksController(_svc.Object);
        var r = await ctl.Patch(id, req, CancellationToken.None);
        r.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_NoContent_WhenServiceReturnsTrue()
    {
        var id = Guid.NewGuid();
        _svc.Setup(s => s.SoftDeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var ctl = new TasksController(_svc.Object);
        var r = await ctl.Delete(id, CancellationToken.None);
        r.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NotFound_WhenServiceReturnsFalse()
    {
        var id = Guid.NewGuid();
        _svc.Setup(s => s.SoftDeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var ctl = new TasksController(_svc.Object);
        var r = await ctl.Delete(id, CancellationToken.None);
        r.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task List_MapsQueryAndClampsValues()
    {
        TaskQuery? captured = null;
        _svc.Setup(s => s.ListAsync(It.IsAny<TaskQuery>(), It.IsAny<CancellationToken>()))
            .Callback<TaskQuery, CancellationToken>((q, _) => captured = q)
            .ReturnsAsync(new TaskListResponse(new List<TaskDto>(), 1, 100, 0));
        var ctl = new TasksController(_svc.Object);

        var result = await ctl.List(page: 0, pageSize: 999, q: " q ", priority: new[] { TaskPriority.High, TaskPriority.Low }, tag: new[] { " work ", "home" }, sort: TaskSortBy.CreatedAt, order: SortOrder.Asc, ct: CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Page.Should().Be(1);
        captured.PageSize.Should().Be(100);
        captured.Q.Should().Be(" q ");
        captured.Priorities!.Should().Contain(new[] { TaskPriority.High, TaskPriority.Low });
        captured.Tags!.Should().Contain(new[] { "work", "home" });
    }

    [Fact]
    public async Task Import_WithValidFile_ReturnsSummary()
    {
        var summary = new TaskImportResult(2, 0, new List<string>(), new List<Guid>());
        _svc.Setup(s => s.ImportAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ReturnsAsync(summary);
        var ctl = new TasksController(_svc.Object);

        var payload = Encoding.UTF8.GetBytes("title,dueDate,priority\nTask,2025-01-01,Med");
        using var ms = new MemoryStream(payload);
        var file = new FormFile(ms, 0, payload.Length, "file", "tasks.csv");

        var result = await ctl.Import(file, CancellationToken.None);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().Be(summary);
    }

    [Fact]
    public async Task Import_NoFile_ReturnsBadRequest()
    {
        var ctl = new TasksController(_svc.Object);
        var result = await ctl.Import(null, CancellationToken.None);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
