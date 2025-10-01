# AGENTS Guidelines

## Project Overview
- TodoApp demonstrates AI-assisted software delivery on a full-stack .NET 8 solution with distinct API, Application, and Infrastructure layers.
- TodoApp.Api hosts ASP.NET Core Web API endpoints, Razor views, and static assets; TodoApp.Application centralizes business rules, validation, and DTOs; TodoApp.Infrastructure encapsulates Entity Framework Core data access over SQLite.
- Domain models are transported via immutable record DTOs and enums that keep the contract consistent from database to UI.
- Client interactions are driven by the module in wwwroot/js/todo.js, which manages DOM state, accessibility hooks, and API calls without leaking globals.
- Automated coverage spans xUnit unit tests with Moq and FluentAssertions, repository tests over ephemeral SQLite connections, and Playwright-powered UI smoke tests.

## Build and Test Commands
- dotnet restore TodoApp.sln
- dotnet build TodoApp.sln
- dotnet run --project src/TodoApp.Api/TodoApp.Api.csproj
- dotnet test TodoApp.sln --logger "console;verbosity=detailed"
- pwsh tests/TodoApp.Tests/bin/Debug/net8.0/playwright.ps1 install   (run once to provision browsers before UI tests)

## Code Style Guidelines
- Target .NET 8 with nullable reference types; prefer asynchronous APIs that accept CancellationToken and use the Async suffix on method names.
- Keep controllers thin by delegating validation and orchestration to the application layer; expose typed responses or descriptive problem payloads.
- Centralize guard clauses in helpers such as ValidateCreate and throw ArgumentException with clear messages when inputs are invalid.
- Favor record types for transport objects, enums for constrained options (priority, sort order), and expression-bodied members for simple pass-through methods.
- Use var where the assigned type is evident, maintain PascalCase for types and methods, camelCase for locals, and keep files focused on a single public type.
- JavaScript should continue using const and let, stay inside the module closure, and reuse shared helpers like showBanner or loadList instead of duplicating DOM logic.
- Tests follow an Arrange-Act-Assert flow, keep a single behavioral expectation per test, and use the MethodName_Scenario_Expectation naming convention.

## Testing Instructions
- Install the .NET 8 SDK, restore dependencies, and run the Playwright browser installer command once before executing UI tests.
- Execute the full suite with dotnet test TodoApp.sln; repository tests create in-memory SQLite databases and require no external services.
- UI tests start the API in the Testing environment with an isolated SQLite file; use PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD or PLAYWRIGHT_BROWSERS_PATH env vars if CI manages browsers separately.
- Target a subset with commands such as dotnet test tests/TodoApp.Tests/TodoApp.Tests.csproj --filter FullyQualifiedName~Application (swap Application for Api, Infrastructure, or Ui as needed).
- Collect coverage via dotnet test TodoApp.sln /p:CollectCoverage=true /p:CoverletOutputFormat=lcov and publish artifacts from tests/TodoApp.Tests/coverage.

## Security Considerations
- Treat all inbound data as untrusted: TaskService validation should remain comprehensive, and CSV imports must continue checking headers, dates, and length limits before persistence.
- Enforce upload size limits on the import endpoint via RequestSizeLimit and reject unexpected media types early in the pipeline.
- Store secrets outside the repo; rely on user secrets or environment variables such as ConnectionStrings__Default for local overrides and ensure CI injects them securely.
- Keep data access behind Entity Framework Core to preserve parameterization; only use raw SQL with explicit safeguards and targeted tests.
- Sanitize any user-generated content before injecting it into the DOM, and retain Playwright regression tests around key flows to surface XSS or auth regressions.

# High Priority Agent Protocol
- All chat responses MUST begin with the prefix: "Hooray, yes sir, on it".