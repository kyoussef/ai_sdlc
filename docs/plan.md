# Todo App — Delivery Plan (from final_requirements.md)

## Stories & Gherkin (INVEST, UI-focused)

- T01 — Create Task (UI Form)
  - As a user, I want to add a new task using a simple form so that I can quickly capture work items.
  - Acceptance
    ```gherkin
    Scenario: Create a valid task via UI
      Given I am on the Tasks page listing tasks
      And no task creation form is visible
      When I click the Add button
      Then the task creation form appears with the Title field focused
      When I type "Buy milk" into Title and choose Priority "Med"
      And I click Save
      Then I see a new list item at the top showing title "Buy milk" and completed = false
      And the form closes and focus returns to the Add button
      And no full page reload occurs
    ```

- T02 — View Tasks (Paginated List)
  - As a user, I want to view tasks in a paginated list so that the UI stays responsive with many items.
  - Acceptance
    ```gherkin
    Scenario: Default pagination shows first page
      Given at least 25 tasks exist
      When I open the Tasks page
      Then I see 20 tasks rendered in the list
      And a pager displays page 1 of 2 with Next enabled and Prev disabled
      When I click Next
      Then page 2 shows the remaining 5 tasks
    ```

- T03 — Edit Task (Inline)
  - As a user, I want to edit a task inline so that I can correct details without leaving the list.
  - Acceptance
    ```gherkin
    Scenario: Edit title inline with save
      Given a visible task with title "Buy milk"
      When I click Edit on that task and change the title to "Buy milk and eggs"
      And I click Save
      Then the list updates to show "Buy milk and eggs" without page reload
      And any validation errors are shown inline near the edited field
    ```

- T04 — Toggle Completed (Checkbox)
  - As a user, I want to mark a task complete via a checkbox so that I can track progress quickly.
  - Acceptance
    ```gherkin
    Scenario: Toggle completed on
      Given a task showing completed = false
      When I check its completion checkbox
      Then the task visually indicates completion (e.g., strike-through)
      And the change persists after refresh
    ```

- T05 — Soft Delete (Confirm Remove)
  - As a user, I want to remove a task from the list with confirmation so that I do not delete by mistake.
  - Acceptance
    ```gherkin
    Scenario: Confirmed delete hides task from list
      Given a visible task "Buy milk"
      When I click Delete and confirm the dialog
      Then the task disappears from the list
      And it stays absent after refresh
    ```

- T06 — Search (Title/Description)
  - As a user, I want to search tasks using a search box so that I can find tasks quickly.
  - Acceptance
    ```gherkin
    Scenario: Search filters by text across title and description
      Given tasks titled "Buy milk" and another with description "Pick up eggs"
      When I type "eggs" into Search
      Then only tasks containing "eggs" in title or description remain visible
      And clearing Search restores the full list
    ```

- T07 — Filter by Priority
  - As a user, I want to filter tasks by priority so that I can focus on urgent items.
  - Acceptance
    ```gherkin
    Scenario: Filter by High priority
      Given tasks of priorities High and Low exist
      When I choose Priority = High
      Then the list shows only tasks with priority High
    ```

- T08 — Filter by Tag
  - As a user, I want to filter tasks by tag so that I can narrow results.
  - Acceptance
    ```gherkin
    Scenario: Filter by tag "home"
      Given tasks with tags ["home"] and ["work"] exist
      When I choose Tag = home
      Then the list shows only tasks containing tag "home"
    ```

- T09 — Combine Filters & Search
  - As a user, I want to combine search, priority, and tag filters so that I can target specific tasks.
  - Acceptance
    ```gherkin
    Scenario: Search + Priority + Tag combined
      Given tasks exist matching combinations of text, priority, and tag
      When I set Search = "report", Priority = Med, Tag = work
      Then only tasks containing "report" with priority Med and tag work are listed
    ```

- T10 — Sort Tasks (Created/Due/Priority)
  - As a user, I want to sort tasks by created date, due date, or priority so that the order suits my review flow.
  - Acceptance
    ```gherkin
    Scenario: Sort by due date ascending
      Given two tasks with due dates 2024-01-01 and 2024-01-10
      When I select Sort = Due Date and Order = Asc
      Then the 2024-01-01 task appears before the 2024-01-10 task
    ```

- T11 — Validation Messages (Inline)
  - As a user, I want clear inline validation so that I can fix form errors quickly.
  - Acceptance
    ```gherkin
    Scenario: Title required on add
      Given the Title field is empty
      When I click Add
      Then I see an inline error near Title stating it is required
      And no task is added to the list
    ```

- T12 — Accessibility & Keyboard Navigation
  - As a user, I want to operate the app via keyboard so that it remains accessible.
  - Acceptance
    ```gherkin
    Scenario: Add task using keyboard only
      Given focus is on Title
      When I enter text and press Tab to the Add button and press Enter
      Then a new task appears at the top of the list
      And keyboard focus returns to Title
    ```

- T13 — Responsive Layout (Mobile)
  - As a mobile user, I want the layout to adapt so that the app remains usable on small screens.
  - Acceptance
    ```gherkin
    Scenario: Mobile layout stacks controls
      Given my viewport width is 360px
      When I open the Tasks page
      Then the form and filters stack vertically without horizontal scroll
      And tap targets are at least 44px high
    ```

- T14 — Error Banner (Network/API Failure)
  - As a user, I want a clear error banner if an action fails so that I know what happened and can retry.
  - Acceptance
    ```gherkin
    Scenario: Show error banner on failed add
      Given the server will reject creates (simulated)
      When I attempt to add a task
      Then a dismissible error banner appears with a human-friendly message
      And the entered title remains in the form for retry
    ```

- T15 — Persist Filters in URL
  - As a user, I want the current search, filter, sort, and page to be reflected in the URL so that I can bookmark/share the view.
  - Acceptance
    ```gherkin
    Scenario: URL reflects and restores state
      Given I set Search = "report", Priority = Med, Tag = work, Sort = DueDate Asc, Page = 2
      When I copy the URL and reload it in a new tab
      Then the list loads with the same search, filters, sort, and page applied
    ```

## Backlog

| ID  | Story                                   | Epic               | Priority | Estimate (No AI) | Estimate (With AI) | Dependencies |
|-----|-----------------------------------------|--------------------|----------|------------------|--------------------|--------------|
| P1  | Platform Foundations (Model/API/Error)  | Platform/NFR       | High     | 5                | 3                  | —            |
| T01 | Create Task (UI Form)                   | CRUD               | High     | 5                | 3                  | P1           |
| T02 | View Tasks (Paginated List)             | CRUD               | High     | 5                | 3                  | P1           |
| T03 | Edit Task (Inline)                      | CRUD               | Medium   | 5                | 3                  | T01,T02      |
| T04 | Toggle Completed (Checkbox)             | State & Updates    | High     | 3                | 2                  | T02          |
| T05 | Soft Delete (Confirm Remove)            | State & Updates    | High     | 3                | 2                  | T02          |
| T06 | Search (Title/Description)              | Filtering & Query  | Medium   | 5                | 3                  | T02          |
| T07 | Filter by Priority                      | Filtering & Query  | Medium   | 3                | 2                  | T02          |
| T08 | Filter by Tag                           | Filtering & Query  | Medium   | 3                | 2                  | T02          |
| T09 | Combine Filters & Search                | Filtering & Query  | Medium   | 3                | 2                  | T06,T07,T08  |
| T10 | Sort Tasks                              | Filtering & Query  | Medium   | 3                | 2                  | T02          |
| T11 | Validation Messages (Inline)            | Usability/NFR      | High     | 3                | 2                  | T01          |
| T12 | Accessibility & Keyboard Navigation     | Usability/NFR      | Medium   | 3                | 2                  | T01,T02      |
| T13 | Responsive Layout (Mobile)              | Usability/NFR      | Medium   | 2                | 1                  | T01,T02      |
| T14 | Error Banner (Network/API Failure)      | Usability/NFR      | Medium   | 3                | 2                  | P1           |
| T15 | Persist Filters in URL                  | Usability          | Low      | 3                | 2                  | T06–T10      |

Notes:
- Estimates are story points. "With AI" assumes using Copilot/Codex for scaffolding and Gemini/ChatGPT for promptable UI/error copy and test skeletons.

## Sprint Plan (2 Sprints, 2 Weeks Each)

- Sprint 1 — Foundations + Core UI CRUD
  - P1 Platform Foundations (DB model, migrations, endpoints, ProblemDetails, base layout)
  - T01 Create Task (UI Form)
  - T02 View Tasks (Paginated List)
  - T11 Validation Messages (Inline)
  - T14 Error Banner (Network/API Failure)
  - Definition of Done: Create/List work end-to-end; errors surfaced in UI; CI builds and runs tests

- Sprint 2 — State, Query, and UX polish
  - T04 Toggle Completed (Checkbox)
  - T05 Soft Delete (Confirm Remove)
  - T03 Edit Task (Inline)
  - T06 Search (Title/Description)
  - T07 Filter by Priority
  - T08 Filter by Tag
  - T09 Combine Filters & Search
  - T10 Sort Tasks
  - T12 Accessibility & Keyboard Navigation
  - T13 Responsive Layout (Mobile)
  - T15 Persist Filters in URL
  - Definition of Done: All flows functional; manual UI checklist passes; perf targets verified

## Risks & Assumptions

- Assumptions
  - Single-user/small-team usage; no auth in MVP
  - Soft delete only; no restore UI in MVP
  - Tags are free text, normalized lowercase; max 10 per task
  - Default sort createdAt desc; default pageSize 20, max 100
  - Date-only due dates; timestamps in UTC; JSON camelCase; ProblemDetails errors

- Risks/Unknowns
  - Scope creep (auth/sharing/notifications) threatens MVP simplicity and timelines
  - SQLite write-locking under concurrent writes may affect perceived stability
  - Search expectations for large datasets may require optimization (indexes/full-text)
  - Accessibility criteria need explicit review to meet WCAG 2.1 basics
  - Free-text tags may hinder consistent filter UX; taxonomy may be needed later
  - Limited automated UI testing increases regression risk; rely on API tests + manual UI checklist
