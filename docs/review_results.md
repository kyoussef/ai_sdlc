# Code Review – import_tasks vs master

## Summary
- High: TaskService.ImportAsync processes the CSV stream line-by-line and cannot cope with quoted multiline fields, so valid user exports that contain line breaks (e.g., descriptions written in Excel) are rejected as "invalid CSV format".
- Medium: Header comparison in the importer does not trim whitespace, so a fairly common CSV header such as "title, dueDate, priority" is rejected because dueDate is not matched after the leading space.
- Overall feature wiring (controller, UI, tests, docs) is cohesive and well covered; once the parser issues are fixed the workflow should be solid.

## Detailed Feedback

### src/TodoApp.Application/Tasks/TaskService.cs
- Positive: Robust validation and error aggregation make it easy to understand which rows failed, and the stream is left open so the controller can dispose it at the right scope.
- Issue (High): Because the importer reads with ReadLineAsync and parses each line independently (src/TodoApp.Application/Tasks/TaskService.cs:104 and src/TodoApp.Application/Tasks/TaskService.cs:262), any field that contains a newline inside quotes will be split across iterations. ParseCsvLine subsequently returns Array.Empty<string>(), surfacing a false "invalid CSV format" error on otherwise valid CSV data (typical for multiline descriptions exported from spreadsheets). Please accumulate lines until quotes balance or switch to TextFieldParser or a CSV library that supports multiline fields.
- Issue (Medium): Header lookup compares the raw header token without trimming (src/TodoApp.Application/Tasks/TaskService.cs:90). A file with headers like "title, dueDate, priority" (note the space Excel often inserts after delimiters) will bail out with "header must contain…" even though the columns are present. Trimming each header before comparison would make the importer resilient to this.

### src/TodoApp.Application/Tasks/Dtos/TaskDtos.cs
- Positive: TaskImportResult cleanly exposes success, failure, and the identifiers of newly created tasks, which is helpful for downstream consumers.
- Suggestion: The XML comment mentions errors being "keyed" by line, but the type is just a list of strings; consider either clarifying the comment or switching to a structured shape (e.g., line number plus message) so that callers do not have to parse text.

### src/TodoApp.Application/Tasks/Interfaces/ITaskService.cs
- Positive: Interface documentation clearly explains the expectations for the new import workflow, keeping implementations aligned.
- Suggestion: Long-running imports can take noticeable time; consider documenting whether callers should expect the method to stream responses or whether it validates the entire file before returning.

### src/TodoApp.Api/Controllers/TasksController.cs
- Positive: The endpoint validates empty uploads early and advertises the new response contract via ProducesResponseType, which is great for client generation.
- Suggestion: When the summary reports zero successful rows and only errors, consider returning a 422 (Unprocessable Entity) instead of 200 so API consumers can distinguish "all rows failed" from "partial success" without inspecting the payload.

### src/TodoApp.Api/Views/Tasks/Index.cshtml
- Positive: The import trigger is added alongside existing filters without disrupting layout, and the hidden file input keeps the markup simple.
- Suggestion: Add aria-controls or an aria-describedby relationship between the button and banner so screen-reader users understand where import status appears.

### src/TodoApp.Api/wwwroot/js/todo.js
- Positive: Nice UX touches—disabling the button, setting aria-busy, and reusing the banner for success/error states—make the flow feel polished.
- Suggestion: Only the first five error messages are surfaced to the user; consider providing a way to review the full error list (e.g., download link or modal) when large imports are processed.

### tests/TodoApp.Tests/Application/TaskServiceTests.cs
- Positive: Unit tests cover both the happy path and validation failures for the importer, ensuring the new service logic is guarded.
- Suggestion: Add a case that exercises the completed column (including the repo patch call) so regressions around status toggling are caught.

### tests/TodoApp.Tests/Api/TasksControllerTests.cs
- Positive: Controller tests validate both success and empty-upload scenarios, locking in the API contract.
- Suggestion: Consider asserting on the serialized problem details returned by the bad request to ensure the client-facing message remains descriptive.

### tests/TodoApp.Tests/Infrastructure/TaskRepositoryTests.cs
- Positive: New tests for due-date and priority sorting give confidence that the UI sort options work end-to-end.
- Suggestion: It may be worth adding a descending due-date case to guard the other branch of the query logic.

### README.md
- Positive: Comprehensive README gives newcomers a clear map of the AI-assisted SDLC flow and how to explore the sample app.
- Suggestion: The document is quite long; a short TL;DR up top for quick-start readers could improve scannability.

### docs/guidelines.md
- Positive: The new walkthrough steps make it easy to reproduce the import scenario and review workflow.
- Suggestion: Step numbering mixes digits and text; consider markdown numbered lists (1.) for consistent formatting.

### data/sample_tasks.csv
- Positive: Including a ready-to-use CSV lowers the barrier for anyone testing the import path.
- Suggestion: Mention this sample file in the README/import instructions so users know it exists.

### docs/import_tasks.png
- Positive: Visual reference helps set expectations for the UI change.
- Suggestion: Document the resolution/intent so future updates know when to refresh the screenshot.

### src/TodoApp.Api/App_Data/todo.db
- Concern: The SQLite file changes by 32KB; committing mutable database files can cause noisy diffs over time. If the schema is unchanged, prefer migrations or seed scripts instead of updating the binary.

