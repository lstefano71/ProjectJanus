## Project Overview
Project Janus is a Windows desktop diagnostic tool for analyzing system events around a critical timestamp. It scans all event logs within a configurable window, presenting a unified, chronological view. Snapshots are saved as portable files for offline analysis.

## Documentation
- **Product Requirements Document:** `docs/prd/projectjanus.md`
- **To-Do List:** `docs/todo.md`

## Developer Workflows
- **Build:** Standard .NET build (`dotnet build`).
- **Run:** Launch via Visual Studio or `dotnet run` from `Janus.App`.
- **Test:** (Add details here if/when tests are present.)
- **Snapshot Files:** Use `.janus` extension. Snapshots are immutable and portable.
