## Project Overview
Project Janus is a Windows desktop diagnostic tool for analyzing system events around a critical timestamp. It scans all event logs within a configurable window, presenting a unified, chronological view. Snapshots are saved as portable files for offline analysis.

## Architecture & Structure
- **Solution Structure:**
  - `Janus.Core`: EF Core models (`ScanSession`, `EventLogEntry`) and DbContext. No UI dependencies.
  - `Janus.App`: Services, ViewModels, Views (WPF/WinUI 3, strict MVVM, no code-behind), and application logic.
- **Tech Stack:**
  - C# 13+, .NET 9+, WPF/WinUI 3 (MVVM only), SQLite (via EF Core, no migrations).
  - All event log access via `System.Diagnostics.Eventing.Reader` (never use legacy `System.Diagnostics.EventLog`).

