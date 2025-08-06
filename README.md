# Project Janus

> **A modern Windows desktop tool for analyzing system event logs around a critical timestamp.**

---

## Overview

Project Janus is a diagnostic utility that scans all Windows event logs within a configurable time window, centered on a timestamp of interest. It presents a unified, chronological view of relevant events, helping you quickly investigate incidents or issues. Snapshots are saved as portable files for offline analysis.

> [!TIP]
> Project Janus is built with modern C# and WPF, using a clean MVVM architecture and EF Core for snapshot storage.

---

## Features

- **Unified Event Log Scanning**: Scans all available event logs in parallel using the modern Windows Event Log API.
- **Configurable Time Window**: Analyze events before and after a central timestamp.
- **Portable Snapshots**: Save and load event log snapshots as portable files for offline review.
- **Modern UI**: Responsive WPF interface with clear separation of concerns (MVVM, no code-behind logic).
- **Progress & Cancellation**: Real-time scan progress and cancellation support.
- **Status Bar**: Displays scan progress and event count.

---

## Getting Started

### Prerequisites

- Windows 10/11
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

### Build & Run

```pwsh
# Clone the repository
# (replace with your actual repo URL)
git clone <your-repo-url>
cd ProjectJanus

# Build the application
dotnet build Janus.App/Janus.App.csproj

# Run the application
dotnet run --project Janus.App
```

Or open `ProjectJanus.sln` in Visual Studio 2022+ and run `Janus.App`.

---

## Project Structure

```
ProjectJanus.sln
├── Janus.Core/         # EF Core models and DbContext (no UI dependencies)
├── Janus.App/          # WPF application (MVVM, services, ViewModels, Views)
├── docs/               # Product requirements, UI mockups, and documentation
└── ...
```

---

## Architecture

- **Janus.Core**: Contains `EventLogEntry`, `ScanSession`, and `EventSnapshotDbContext` for snapshot storage. No UI dependencies.
- **Janus.App**: Implements services for event log scanning, snapshot management, and user settings. MVVM pattern with ViewModels and Views for live scanning, results, and welcome screens.

---

## Usage

1. Launch Project Janus.
2. Select or enter the timestamp of interest.
3. Configure the time window (before/after the timestamp).
4. Start the scan to view all relevant events in a unified timeline.
5. Save snapshots for offline analysis as needed.

---

## Documentation

- Product requirements: [`docs/prd/ProjectJanus.md`](docs/prd/ProjectJanus.md)
- To-Do list: [`docs/TODO.md`](docs/TODO.md)

> [!NOTE]
> This README describes the current implementation. For planned features and design details, see the documentation above.

---

## Acknowledgements

Project Janus is inspired by best practices in modern .NET desktop development and the needs of IT professionals investigating system events.
