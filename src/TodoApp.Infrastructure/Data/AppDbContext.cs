using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using TodoApp.Infrastructure.Data.Entities;

namespace TodoApp.Infrastructure.Data;

/// <summary>
/// EF Core database context backing the Todo application.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Task aggregate set with a global query filter for soft deletes.
    /// </summary>
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    /// <summary>
    /// Initializes a new <see cref="AppDbContext"/> using supplied options.
    /// </summary>
    /// <param name="options">EF Core configuration options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>
    /// Applies entity configuration including conversions and indexes.
    /// </summary>
    /// <param name="modelBuilder">Model builder provided by EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TaskItemConfig());
    }
}

internal class TaskItemConfig : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> b)
    {
        b.ToTable("Tasks");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.Priority).HasConversion<int>();

        // Custom comparer keeps EF Core change tracking stable for JSON-serialized tag collections.
        var tagsComparer = new ValueComparer<List<string>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            c => c == null ? 0 : c.Aggregate(0, (acc, v) => HashCode.Combine(acc, v == null ? 0 : v.GetHashCode())),
            c => c == null ? new List<string>() : c.ToList());

        b.Property(x => x.Tags)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .Metadata.SetValueComparer(tagsComparer);

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt).IsRequired();
        b.Property(x => x.DeletedAt);
        b.Property(x => x.RowVersion)
            .IsRequired()
            .IsConcurrencyToken()
            .ValueGeneratedNever();

        b.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_Tasks_CreatedAt");
        b.HasIndex(x => x.DueDate).HasDatabaseName("IX_Tasks_DueDate");
        b.HasIndex(x => x.Priority).HasDatabaseName("IX_Tasks_Priority");

        b.HasQueryFilter(x => x.DeletedAt == null);
    }
}
