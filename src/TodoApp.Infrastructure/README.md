# TodoApp.Infrastructure

## Purpose
Implements the persistence layer using EF Core and SQLite, supplying repository implementations, entity configurations, migrations, and seed data for the Todo App.

## Data Access Components
- **AppDbContext**
  - Configures the `Tasks` DbSet with JSON-serialized tag collection, concurrency token (`RowVersion`), indexes, and a global soft-delete query filter.
  - `TaskItemConfig` uses a custom `ValueComparer` to correctly detect `List<string>` changes after JSON conversion.
- **TaskRepository**
  - Executes filtered queries:
    - Free-text search via `EF.Functions.Like` on title/description.
    - Priority filters translated to integer enum values.
    - Tag filters applied in-memory (SQLite cannot query JSON arrays efficiently); OR semantics ensure tasks match any selected tag.
  - Centralizes pagination and default ordering (`CreatedAt desc`).
  - Manages row-version concurrency by assigning `Guid` bytes each mutation and setting `OriginalValues` when clients supply tokens.
  - Returns DTOs, keeping EF entities internal to the layer.
- **DbSeeder** populates sample records on first run for demos and automated tests.

## Entities & Migrations
- **TaskItem** mirrors DTO properties plus persistence-specific metadata (`RowVersion`, `DeletedAt`).
- Initial migration (`20250922070049_InitialCreate`) creates the `Tasks` table with indexes listed below.

| Index | Columns | Purpose |
| --- | --- | --- |
| IX_Tasks_CreatedAt | CreatedAt | Sort by recency |
| IX_Tasks_DueDate | DueDate | Filter upcoming work |
| IX_Tasks_Priority | Priority | Filter critical work |

## Configuration & DI
- `InfrastructureServiceCollectionExtensions.AddInfrastructure` wires `AppDbContext` and `TaskRepository` into the service container. Provide `ConnectionStrings:Default` so the SQLite provider can connect.
- In-memory provider support can be added by extending the method to fall back to `UseSqlite("Data Source=:memory:")` in tests.

## Testing Tips
- Use `SqliteConnection("DataSource=:memory:")` for fast repository integration tests; remember to keep the connection open for EF.
- Seed with `DbSeeder` or custom fixtures before exercising repository behavior.
- Verify concurrency by simulating stale `RowVersion` values and asserting `DbUpdateConcurrencyException`.

## Operational Considerations
- Tag filtering currently loads candidate rows into memory; monitor performance if the dataset grows and consider FTS or JSON1 extensions for SQLite.
- Soft deletes retain data indefinitely; add archival jobs or scheduled purges if the table grows large.
- Introduce migration tooling (e.g., GitHub Actions + `dotnet ef database update`) to automate schema deployment.
