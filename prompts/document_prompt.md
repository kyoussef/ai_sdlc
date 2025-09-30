# Code Documentation Generation Request

Your task is to create comprehensive documentation for the Todo App project located in the `src/` directory. The project follows a clean architecture pattern with three main layers: **Api**, **Application**, and **Infrastructure**.

## Project Context
- **Framework**: .NET 8 WebAPI with Entity Framework Core
- **Architecture**: Layered architecture (Presentation → Application → Infrastructure)
- **Database**: SQLite with EF Core migrations
- **UI**: Razor Views + vanilla JavaScript
- **Testing**: xUnit, FluentAssertions, Moq, Playwright

## Documentation Requirements

### 1. Inline Code Comments
- **Controllers**: Add XML comments explaining endpoint purposes, parameters, and return types
- **Services**: Document business logic, validation rules, and error conditions
- **Repositories**: Explain query logic, filtering, and data transformations
- **Middleware**: Document request/response pipeline behavior
- **Entity Models**: Explain property purposes, relationships, and constraints

**Style Guidelines:**
- Use clear, concise language
- Explain the "why" not just the "what"
- Include parameter validation and error scenarios
- Document any complex business rules or algorithms

### 2. XML Documentation (DocStrings)
Generate complete XML documentation for:
- **Public classes and interfaces**
- **Public methods and properties**
- **Constructor parameters**
- **Generic type parameters**
- **Exceptions that can be thrown**

### 3. API Documentation (Swagger/OpenAPI)
- **Endpoint summaries**: Clear descriptions of what each endpoint does
- **Parameter documentation**: Explain query parameters, path parameters, and request bodies
- **Response schemas**: Document success and error response formats
- **Example requests/responses**: Provide realistic sample data
- **Error codes**: Document all possible HTTP status codes and their meanings

### 4. README Files
Create the following README files:

#### **Main Project README** (`src/README.md`)
Include:
- Project overview and purpose
- Architecture diagram (use Mermaid if possible)
- Technology stack and dependencies
- Local development setup instructions
- API endpoint summary with example requests
- Database schema overview
- Deployment instructions

#### **Layer-Specific READMEs**
- **`src/TodoApp.Api/README.md`**: Controllers, middleware, UI components
- **`src/TodoApp.Application/README.md`**: Services, validators, business logic
- **`src/TodoApp.Infrastructure/README.md`**: Data access, repositories, entities

### 5. Architecture Documentation
- **Data Flow Diagrams**: Show request/response flow through layers
- **Database Schema**: Document entities, relationships, and indexes
- **API Contract**: Complete OpenAPI specification
- **Error Handling**: Document error codes, ProblemDetails format, and exception mapping

### 6. Code Examples and Usage Patterns
Include practical examples for:
- **Creating a new task** (full API workflow)
- **Searching and filtering** (query parameter usage)
- **Error handling** (validation failures, not found scenarios)
- **UI interactions** (JavaScript event handling)

## Specific Areas Requiring Attention

### Business Logic Documentation
- Task validation rules (title length, priority values, tag constraints)
- Soft delete behavior and query filtering
- Concurrency handling with row versioning
- Search functionality (case-insensitive, title/description matching)

### API Patterns Documentation
- RESTful conventions and HTTP status codes
- Request/response formats (JSON camelCase)
- Pagination pattern (`page`, `pageSize`, `total`)
- Error responses (RFC7807 ProblemDetails)

### UI Component Documentation
- Razor partial usage patterns
- JavaScript module organization
- Progressive enhancement approach
- Accessibility features and keyboard navigation

## Output Format
Please organize the documentation as follows:
1. **Updated source files** with inline comments and XML documentation
2. **README files** in appropriate directories
3. **API documentation** (OpenAPI YAML or enhanced controller attributes)
4. **Architecture documentation** with diagrams where helpful

Ensure all documentation is:
- **Clear and concise** for developers of varying experience levels
- **Technically accurate** and consistent with the codebase
- **Actionable** with concrete examples and usage patterns
- **Maintainable** and easy to update as code evolves