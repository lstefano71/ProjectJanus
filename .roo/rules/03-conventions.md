## Project Overview
Project Janus is a Windows desktop diagnostic tool for analyzing system events around a critical timestamp. It scans all event logs within a configurable window, presenting a unified, chronological view. Snapshots are saved as portable files for offline analysis.

## Documentation
- **Product Requirements Document:** `docs/prd/projectjanus.md`
- **To-Do List:** `docs/todo.md`

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
