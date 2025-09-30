# Todo App — Final Requirements (MVP)

This document consolidates the initial high‑level requirements with the outcomes of clarifying questions to finalize scope and decisions for the MVP.

## Summary
- Build a simple Todo app for individuals/small teams.
- Deliver a minimal, maintainable MVP quickly (target ~2 sprints).

## MVP Scope
- Tasks: create, read, update, soft delete; toggle completed.
- Fields: title (required), description (optional), due date (optional), priority (low/med/high), tags (0..n).
- List: search (`q`), filter (priority/tag), basic sort; pagination.
- Single UI page (list + create + filters), minimal interactions.

## Non‑Functional Requirements
- Fast load (<1s typical), responsive UI.
- Reliable CRUD with graceful errors (ProblemDetails JSON).
- Basic accessibility (keyboard, contrast) aligned to WCAG 2.1 AA basics.
- Simple observability (structured logs, `/health`).

## Technology Constraints
- Backend: .NET 8 WebAPI, EF Core, SQLite (dev; prod acceptable for single instance in MVP).
- UI: Razor Views + vanilla JS (fetch), static assets in `wwwroot`.
- Single deployable app serving API and views.

## Out of Scope (MVP)
- Authentication/roles, multi‑tenant, real‑time, notifications.
- Attachments/comments, offline sync, mobile apps.
- Bulk operations (bulk delete/complete/update).

---

## Clarifications and Final Decisions

- Soft delete
  - Implement soft delete in MVP. No restore UI in MVP. Deleted items excluded from all queries. Purge/restore considered post‑MVP.

- Tags
  - Free‑text tags; no tag management UI in MVP. Normalize to lowercase. Max 10 tags/task; each tag max 30 chars; allowed: letters, digits, hyphen, underscore.

- Search (`q`)
  - Case‑insensitive search over title and description. Tags are filtered via a dedicated `tag` parameter, not included in `q`.

- Sorting
  - Default sort: `createdAt desc`. Supported fields: `createdAt|dueDate|priority`. Direction via `order=asc|desc`. Stable tie‑break by `id`.

- Pagination
  - 1‑based `page` (default 1) and `pageSize` (default 20, max 100). Response includes paging metadata: `page`, `pageSize`, `total`, `items`.

- PATCH semantics
  - Partial updates allowed for: `title`, `description`, `dueDate`, `priority`, `tags`, `completed`. Toggling `completed` uses PATCH `{ completed: boolean }`.

- Due date
  - Date‑only (ISO `YYYY-MM-DD`). Stored and displayed as date (no timezone math). Past due dates allowed.

- Validation
  - `title` required (1–200 chars). `description` max 1000 chars. `priority` ∈ {low, med, high}. `tags` follow the constraints above. Unknown fields rejected. Validation errors return ProblemDetails with per‑field error map.

- Filter combinations
  - Filters are combinable (`q` + `priority` + `tag`). Empty results return 200 with empty `items`.

- List vs Detail payloads
  - List item fields: `id`, `title`, `completed`, `priority`, `dueDate`, `tags`, `createdAt`, `updatedAt`.
  - Detail adds: `description`; `deletedAt` is included in detail but null for active tasks.

- IDs and idempotency
  - Server generates GUID v4 IDs. Client‑provided IDs ignored. No idempotency keys for create in MVP. DELETE and PATCH are idempotent.

- Browsers & Accessibility
  - Support latest 2 versions of Chrome, Edge, Firefox, Safari. Responsive layout for mobile widths ≥360px. Keyboard operability, focus states, labels/roles, 4.5:1 color contrast.

- Performance targets
  - Initial HTML load < 1s on broadband. P95 API latency < 150ms for CRUD and list (pageSize ≤ 20) on reference hardware.

- Environments & DB
  - Dev/Test: SQLite. Prod (MVP): SQLite acceptable for single‑instance. Plan migration path to Postgres in Phase 2.

- Logging & Tracing
  - Structured JSON logs. Correlation ID from `X-Request-ID` (generate if missing) included in logs and echoed in responses.

- Error responses
  - RFC7807 ProblemDetails with `type`, `title`, `status`, `detail`, `instance`, and `errors` (per‑field validation map).

- Data retention & export
  - No export in MVP. Retain data indefinitely for MVP. Backups handled at environment/platform level.

- Hosting & Security
  - Linux container, Kestrel behind reverse proxy. Same‑origin only (CORS disabled). TLS terminated upstream. Security headers: HSTS, X‑Content‑Type‑Options, X‑Frame‑Options DENY, minimal CSP (self only) for MVP.

---

## Resulting API Conventions

- List endpoint
  - `GET /api/tasks?page=1&pageSize=20&q=<text>&priority=low|med|high&tag=<tag>&sort=createdAt|dueDate|priority&order=asc|desc`
  - Response body: `{ items: [...], page, pageSize, total }`

- Item endpoints
  - `POST /api/tasks` (create)
  - `GET /api/tasks/{id}` (detail)
  - `PUT /api/tasks/{id}` (replace)
  - `PATCH /api/tasks/{id}` (partial update, e.g., `{ completed: true }`)
  - `DELETE /api/tasks/{id}` (soft delete)

- Errors
  - ProblemDetails for 400 (validation), 404 (not found), 409 (concurrency/conflict).

---