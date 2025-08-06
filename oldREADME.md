# Project Janus

> [!TIP]
> **Project Janus** is a Windows desktop diagnostic tool for rapid, unified analysis of system events around a critical timestamp. It empowers developers, sysadmins, and power users to pinpoint root causes of failures in seconds, not hours.

---

## Overview

Diagnosing transient issues on Windows is often slow and error-prone. Project Janus streamlines this process by scanning all event logs within a configurable window, presenting a unified, chronological view. Snapshots can be saved as portable files for offline or collaborative analysis.

**Key Features:**
- **Unified Event Timeline:** See all relevant system and application events before and after a critical moment.
- **Portable Snapshots:** Save and share `.janus` snapshot files for offline analysis—no dependencies on the source machine.
- **Intuitive UI:** Clean, responsive interface with real-time progress, filtering, and color-coded severity.
- **Asynchronous Scanning:** Fast, cancellable scans that never freeze the UI.
- **Rich Filtering & Export:** Search, filter by severity, and export results to CSV for reporting.

---

## Getting Started

> [!IMPORTANT]
> Project Janus requires administrative privileges to access all event logs. Launching the app will prompt for elevation via UAC.

### Prerequisites
- Windows 10/11
- .NET 9+ Runtime

### Build & Run
1. **Clone the repository:**
   ```pwsh
   git clone <this-repo-url>
   cd ProjectJanus
   ```
2. **Build the application:**
   ```pwsh
   dotnet build Janus.App/Janus.App.csproj
   ```
3. **Run the app:**
   ```pwsh
   dotnet run --project Janus.App
   ```
   Or launch `Janus.App` from Visual Studio.

---

## Usage

1. **Welcome Screen:**
   - Choose **New Live Scan** or **Open Snapshot...**
   - Recent snapshots are listed for quick access.
2. **Configure Scan:**
   - Select the timestamp of interest and time window (minutes before/after).
   - Click **Scan Events**. Progress and event count are shown live.
   - Cancel at any time; partial results are displayed.
3. **Review Results:**
   - Events are shown in a sortable, color-coded grid (Error/Critical in red, Warning in yellow).
   - Select an event to view full details.
   - Filter by severity or search text.
   - Right-click to export to CSV or copy event details.
4. **Save/Load Snapshots:**
   - Save results as a `.janus` file (with optional notes).
   - Open snapshots for offline analysis—no source machine required.

---

## Architecture

- **Tech Stack:** C# 13, .NET 9+, WPF/WinUI 3 (MVVM), SQLite (EF Core, no migrations)
- **Core Projects:**
  - `Janus.Core`: Data models (`ScanSession`, `EventLogEntry`), EF Core context
  - `Janus.App`: UI, services, view models, and application logic
- **Event Log Access:** Uses `System.Diagnostics.Eventing.Reader` for robust, parallel log queries
- **Snapshots:** Self-contained SQLite files, fully portable

> [!NOTE]
> All event log access is fully asynchronous and cancellable. The UI remains responsive at all times.

---

## Example Workflow

```plaintext
+--------------------------------------------------------------------+
| Event Pinpointer                                                   |
+--------------------------------------------------------------------+
|  Timestamp of Interest: [ 08/05/2025 ▼] [ 09:57:00 AM ▼]           |
|  Time Window:                                                      |
|    Minutes Before: [ 5 ▼▲]      Minutes After: [ 5 ▼▲]             |
|                                [ Scan Events ]                     |
| +----------------------------------------------------------------+ |
| | Results (153 events found) Filter: [Error] [Search: driver]    | |
| +----------------------------------------------------------------+ |
| | Timestamp         | Level   | Source     | Event ID | Log Name | |
| | ----------------- | ------- | ---------- | -------- | -------- | |
| | 09:55:10 AM       | Error   | Kernel-PnP | 219      | System   | |
| | 09:56:59 AM       | Warning | e1dexpress | 27       | System   | |
| | 09:57:01 AM       | Info    | MyApp.exe  | 1001     | App      | |
| | ...               | ...     | ...        | ...      | ...      | |
| +----------------------------------------------------------------+ |
| | Event Details:                                                 | |
| | +------------------------------------------------------------+ | |
| | | The driver \Driver\WudfRd failed to load for the device... | | |
| | +------------------------------------------------------------+ | |
| +----------------------------------------------------------------+ |
+--------------------------------------------------------------------+
```

---

## Resources
- [Product Requirements](docs/prd/ProjectJanus.md)
- [To-Do List](docs/TODO.md)
- [UI Mockup](docs/ui_mockup.txt)

> [!TIP]
> For questions, see the documentation or open an issue.

---

## Acknowledgements
Project Janus is developed by WildHeart Productions. Inspired by the Roman god Janus, looking to both past and future to illuminate the present.
