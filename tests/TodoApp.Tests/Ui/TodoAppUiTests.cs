using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace TodoApp.Tests.Ui;

public class TodoAppUiTests
{
    [Fact]
    public async Task AddTask_ShowsTaskInList()
    {
        await WithTodoAppPage(async page =>
        {
            var title = $"UI Task {Guid.NewGuid():N}";
            var dueDate = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd");

            await page.GotoAsync("/Tasks");
            await page.ClickAsync("#add-task-btn");
            await page.FillAsync("#title", title);
            await page.FillAsync("#dueDate", dueDate);
            await page.FillAsync("#tags", "home,work");
            await page.ClickAsync("#save-task-btn");

            await page.WaitForSelectorAsync("#taskModal", new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Hidden
            });

            await page.WaitForSelectorAsync($"#task-list tr:has-text(\"{title}\")");
        });
    }

    [Fact]
    public async Task Search_ForExistingTask_FiltersResults()
    {
        await WithTodoAppPage(async page =>
        {
            await page.GotoAsync("/Tasks");
            await page.WaitForSelectorAsync("#task-list tr");

            await page.FillAsync("#search", "milk");

            var rows = page.Locator("#task-list tr");
            await Microsoft.Playwright.Assertions.Expect(rows).ToHaveCountAsync(1);
            await Microsoft.Playwright.Assertions.Expect(rows).ToContainTextAsync(new[] { "Buy milk" });
        });
    }

    private static async Task WithTodoAppPage(Func<IPage, Task> test)
    {
        await using var server = await TodoAppUiFactory.StartAsync();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = server.RootUri.ToString(),
            IgnoreHTTPSErrors = true
        });
        var page = await context.NewPageAsync();

        try
        {
            await test(page);
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
