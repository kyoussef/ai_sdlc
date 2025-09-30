# TodoApp.Api

## Purpose
Presentation layer that exposes REST endpoints, Razor views, and middleware for cross-cutting concerns such as correlation IDs, security headers, and RFC7807 error responses.

## Controllers
- **TasksController** (`/api/tasks`)
  - Supports list, detail, create, replace, patch, and delete operations.
  - Validates paging (`1-100` page size), search term trimming, priority/tag filters, and leverages `ProblemDetailsMiddleware` for standardized errors.
  - Emits `ProducesResponseType` metadata to drive Swagger and client generation.
- **TasksWebController** (`/tasks`)
  - Serves the Razor UI shell used by the SPA-like experience.
  - Keeps routing separate so API responses and HTML don't mix.
- **HomeController**
  - Owns marketing/utility pages and the shared error view.

## Middleware
- **CorrelationIdMiddleware** adds/propagates `X-Request-ID` headers and Activity tags for distributed tracing.
- **ProblemDetailsMiddleware** catches `ArgumentException`, `DbUpdateConcurrencyException`, and unhandled errors, replying with `application/problem+json` payloads.
- **SecurityHeadersMiddleware** hardens responses with `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, and disables legacy XSS filters.

Pipeline order: correlation → problem details → security headers → static files → MVC routing. Remember to extend this list when introducing authentication or rate limiting.

## Razor Views & UI Components
- **`Views/Tasks/Index.cshtml`** renders the task board scaffold (filters, table, modals). Partial views `_CreateTaskForm` and `_TaskList` isolate form markup and table body for reuse/testing.
- **Progressive enhancement**: HTML renders fast for non-JS clients; `wwwroot/js/todo.js` hydrates the page, wires bootstrap modals, and communicates with the API using `fetch` + JSON.
- **Accessibility**: Semantic headings, labelled controls, focus management inside modals, keyboard-friendly dropdowns, and ARIA attributes ensure compliance.
- **State management**: `todo.js` keeps query state (page, sort, filters) in a central object, debounces search, mirrors dropdown selections, and re-renders list rows after CRUD events.

## API Documentation
- Enhanced controller attributes feed Swagger generators.
- Canonical OpenAPI document lives at `openapi.yaml` (manually curated for SDK generation and contract testing).

## Testing Hooks
- Controllers are thin; unit tests should target the service layer with mocked repositories.
- For API integration tests, host `TodoApp.Api` with `WebApplicationFactory` and assert against `ProblemDetails` output and headers.
- UI flows can be exercised end-to-end with Playwright against `/tasks`.

## Future Enhancements
- Add authentication middleware.
- Extend `SecurityHeadersMiddleware` with CSP once asset domains are fixed.
- Expose Swagger UI in development by adding `app.UseSwagger()` and `app.UseSwaggerUI()`.
