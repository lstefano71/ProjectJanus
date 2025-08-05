# Copilot Instructions for Project Janus

## Project Overview
Project Janus is a Windows desktop diagnostic tool for analyzing system events around a critical timestamp. It scans all event logs within a configurable window, presenting a unified, chronological view. Snapshots are saved as portable files for offline analysis.

## Documentation
- **Product Requirements Document:** `docs/prd/projectjanus.md`
- **To-Do List:** `docs/todo.md`

## Instructions to AI agents
- never guess an API: use the Context7 tool to explore APIs and their capabilities.
- do not communicate via comments in the code: use clear, descriptive names and documentation files instead.
- always follow the conventions and patterns outlined below.


## Architecture & Structure
- **Solution Structure:**
  - `Janus.Core`: EF Core models (`ScanSession`, `EventLogEntry`) and DbContext. No UI dependencies.
  - `Janus.App`: Services, ViewModels, Views (WPF/WinUI 3, strict MVVM, no code-behind), and application logic.
- **Tech Stack:**
  - C# 13+, .NET 9+, WPF/WinUI 3 (MVVM only), SQLite (via EF Core, no migrations).
  - All event log access via `System.Diagnostics.Eventing.Reader` (never use legacy `System.Diagnostics.EventLog`).

## Key Conventions & Patterns
- **Modern C# Only:**
  - Use primary constructors, `required` properties, collection expressions, file-scoped types, top-level statements, and advanced pattern matching.
- **EF Core Usage:**
  - Do NOT use migrations. Create schema with `DbContext.Database.EnsureCreatedAsync()`.
  - Handle schema mismatches with try/catch on `Microsoft.Data.Sqlite.SqliteException`.
- **Async by Default:**
  - All I/O and long-running operations must be fully async (`async`/`await`).
  - Always accept and respect `CancellationToken` in services.
- **Admin Elevation:**
  - Handled via `app.manifest` (`requireAdministrator`). No code for self-elevation.

## Developer Workflows
- **Build:** Standard .NET build (`dotnet build`).
- **Run:** Launch via Visual Studio or `dotnet run` from `Janus.App`.
- **Test:** (Add details here if/when tests are present.)
- **Snapshot Files:** Use `.janus` extension. Snapshots are immutable and portable.

## UI/UX Principles
- Clean, intuitive, and responsive UI.
- Progress and cancellation always available during scans.
- Status bar shows scan progress and event count.

## Examples
- **Event Log Scanning:**
  - Use `EventLogSession.GlobalSession` to enumerate logs.
  - Query events with XPath by `TimeCreated`.
  - Parallelize log queries, handle `EventLogException` per log.
- **MVVM:**
  - ViewModels invoke services asynchronously, bind commands for scan/cancel.
  - No code-behind in Views.

## References
- See `docs/prd/ProjectJanus.md` and `docs/TODO.md` for requirements and implementation phases.

---
**If any conventions or workflows are unclear, ask for clarification or examples from the maintainers.**
