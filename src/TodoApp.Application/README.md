# TodoApp.Application

## Role
Application layer orchestrating use cases, enforcing validation, and exposing transport-agnostic contracts (DTOs) consumed by controllers, tests, and future clients.

## Services
- **TaskService**
  - Delegates persistence to `ITaskRepository` while enforcing business rules:
    - Title required, trimmed, and limited to 200 characters.
    - Description optional, <= 1000 characters.
    - Create requests must include a due date; update/patch may clear it.
    - Tags optional, trimmed down to 10 entries.
    - Concurrency relies on `RowVersion` (base64 GUID) when present.
  - Wraps repository operations and returns relative resource URIs for newly created tasks.

## Contracts & DTOs
- **TaskPriority** / **SortOrder** enums align with UI filters; serialized values match request expectations (`Low`, `Med`, `High`).
- **CreateTaskRequest / UpdateTaskRequest / PatchTaskRequest** capture validation requirements and concurrency tokens.
- **TaskListResponse** transports pagination metadata, enabling consistent UI/SDK implementations.
- DTO documentation doubles as authoritative schema for API clients and test data builders.

## Interfaces
- **ITaskService** is the façade targeted by controllers; use it in unit tests to assert business outcomes without touching EF Core.
- **ITaskRepository** abstracts persistence, allowing in-memory or mock implementations for testing.

## Business Logic Highlights
- **Search**: `TaskQuery.Q` enables case-insensitive `LIKE` against title and description.
- **Filtering**: Priority filters use OR semantics; tag filters require at least one match and are trimmed for whitespace.
- **Sorting & Pagination**: All list operations normalize page/pageSize and default to `createdAt desc` for stable ordering.
- **Soft Delete**: `SoftDeleteAsync` only timestamps `DeletedAt`, letting the repository-layer filter hide removed items.
- **Concurrency**: Update/Patch validate the caller-provided `RowVersion`. EF raises `DbUpdateConcurrencyException`, converted into HTTP 409 by middleware.

## Testing Guidance
- Mock `ITaskRepository` to simulate success, not-found, and concurrency scenarios.
- Consider contract tests that serialize each DTO to verify camelCase naming and enum handling.
- Use `TaskQuery` test fixtures to cover paging, tag filtering, and defaulting logic.

## Extension Points
- Introduce FluentValidation or data annotations if more complex validation emerges.
- Add domain events when cross-service integrations are required.
