using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Middleware;
using TodoApp.Api.Startup;
using TodoApp.Infrastructure.Startup;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();

// App layers
builder.Services.AddApiServices();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Default"));

var app = builder.Build();

// DB init (apply migrations)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoApp.Infrastructure.Data.AppDbContext>();
    db.Database.Migrate();
    var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("DbSeeder");
    await TodoApp.Infrastructure.Data.DbSeeder.SeedAsync(db, logger);
}

// Middleware
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ProblemDetailsMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

// Endpoints
app.MapHealthChecks("/health");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
