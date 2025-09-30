**Generate High‑Level Technical Design and Architecture (Todo App)**

Context
*   Read and ground your answer in:
    *   docs/final_requirements.md 
    *   docs/plan.md     
*   MVP: .NET 8 WebAPI + Razor Views, vanilla JS (no frontend frameworks), EF Core + SQLite (dev, single instance acceptable in prod for MVP), ProblemDetails errors, soft delete, pagination/filter/sort/search, accessibility basics.

Goal

*   Produce a practical, high-level technical design and architecture proposal to implement the MVP efficiently and safely, aligned to the stories and sprint plan.
    
Deliverables

*   Tech stack and rationale
    *   Backend: .NET 8 WebAPI, EF Core (SQLite), validation, mapping, logging 
    *   UI: Razor Views + partials, vanilla JS modules, fetch wrapper 
    *   Tooling: testing libraries, minimal dev/prod ops; optional libs should be justified
        
*   Architecture and layering
    *   Proposed layers (e.g., Presentation, Application/Services, Infrastructure/Data) and dependencies between them
    *   Responsibilities per layer, boundaries, dependency injection plan
    *   Component/module breakdown (Controllers, Services, Repositories, DbContext, Validators, Middleware)
*   Data model and persistence
    *   Entities and fields (TaskItem, soft delete via DeletedAt, RowVersion concurrency)
    *   EF Core configuration (keys, indexes, query filters to exclude soft‑deleted)
    *   Migration strategy; portability to Postgres later
*   API design conventions
    *   Endpoints aligned to requirements (CRUD + PATCH for completed) 
    *   Query params for pagination/sort/filter/search 
    *   Error handling (RFC7807 ProblemDetails); validation approach 
    *   Versioning stance for MVP 
    *   Generate OpenAPI 3.1 YAML documentation. 
*   UI architecture (Razor + vanilla JS)
    *   Pages/Views/Partials structure (Tasks/Index, list partial, form modal/inline per T01 flow)
    *   JS modules: API client (fetch wrapper), state sync (URL reflects filters), event handling
    *   Progressive enhancement, accessibility, responsive layout guidelines
    *   How T01 workflow operates (Add opens form → Save persists → list refreshes without full reload)
*   Cross‑cutting concerns and best practices
    *   Validation (FluentValidation or model validation), DTO mapping strategy
    *   Error middleware mapping domain exceptions to ProblemDetails
    *   Logging/tracing (structured logs, correlation ID), configuration   
    *   Security headers, CORS (same‑origin), health endpoint, rate/size limits      
    *   Performance: indexes, pagination defaults, N+1 avoidance, caching stance (none for MVP)
        
*   Testing strategy
    *   Unit tests (services/validators)  
    *   Integration tests using WebApplicationFactory for endpoints and ProblemDetails
    *   Lightweight manual UI checklist; optional Playwright later (out of MVP)
            
*   Diagrams (use Markdown Mermaid)
    *   Layered architecture diagram 
    *   Component diagram (Controllers → Services → Repos → DbContext; Views + wwwroot/js)  
    *   Sequence diagram for T01 create flow (UI Add → form → Save → API → DB → UI update)
    
Output Format
*   Title: “High‑Level Technical Design — Todo App (MVP)” 
*   Use Markdown with clear sections matching the Deliverables above.  
*   Include at least:
    *   One layered architecture diagram (Mermaid) 
    *   One component diagram (Mermaid)   
    *   One sequence diagram for T01 (Mermaid)
     
Style
*   Concise, actionable, and opinionated with rationale. Highlight best practices and call out where simplicity is chosen deliberately for MVP.