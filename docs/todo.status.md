# Project Janus – TODO Status (as of 2025-08-06)

This file provides a detailed status review of all items from [`docs/TODO.md`](docs/TODO.md), based on actual codebase analysis.

---

## Phase 1: Project Foundation & Core Data Model

- **1.1:** Set up solution structure: `ProjectJanus.sln`.  
  **Status:** ✅ Complete

- **1.2:** Create main application project (WPF or WinUI): `Janus.App`.  
  **Status:** ✅ Complete

- **1.3:** Create class library for data models: `Janus.Core`.  
  **Status:** ✅ Complete

- **1.4:** Add NuGet packages to `Janus.Core`: `Microsoft.EntityFrameworkCore.Sqlite`.  
  **Status:** ✅ Complete

- **1.5:** Define data model class `EventLogEntry.cs` in `Janus.Core`.  
  **Status:** ✅ Complete

- **1.6:** Define data model class `ScanSession.cs` in `Janus.Core`.  
  **Status:** ✅ Complete

- **1.7:** Implement EF Core context `EventSnapshotDbContext.cs` in `Janus.Core`.  
  **Status:** ✅ Complete

- **1.8:** Add `app.manifest` to `Janus.App` and configure `requestedExecutionLevel` to `requireAdministrator`.  
  **Status:** ✅ Complete

- **1.9:** Initialize a Git repository and create a standard `.gitignore` file for .NET projects.  
  **Status:** ✅ Complete

---

## Phase 2: Live Event Scanning Engine

- **2.1–2.8:** All items implemented in [`Janus.App/Services/EventLogScannerService.cs`](Janus.App/Services/EventLogScannerService.cs).  
  **Status:** ✅ Complete

---

## Phase 3: UI - Live Scan & Control

- **3.1:** Design the application's main shell window (`MainWindow.xaml`).  
  **Status:** 🟡 In Progress  
  **Notes:** Status bar area for progress/event count missing.

- **3.2:** Create a `LiveScanView.xaml` UserControl for the initial scan configuration screen.  
  **Status:** ✅ Complete

- **3.3:** Add UI controls to `LiveScanView`: `DatePicker`, time control, "Now" button, numeric controls for time window.  
  **Status:** ✅ Complete

- **3.4:** Add "Scan" and "Cancel" buttons to `LiveScanView`.  
  **Status:** ✅ Complete

- **3.5:** Add a status bar area to the UI for progress text and event count.  
  **Status:** 🟡 In Progress  
  **Notes:** Only a ScanStatus TextBlock is present; no dedicated status bar area.

- **3.6–3.9:** All other items complete.

---

## Phase 4: UI - Results Display & Interaction

- **4.1–4.9:** All items complete.

- **4.10:** Add a right-click context menu to the `DataGrid` with a "Copy Message" command.  
  **Status:** 🟡 In Progress  
  **Notes:** "Copy Message" is available via button/shortcut, but not via DataGrid context menu.

---

## Phase 5: Snapshot Persistence & Offline Mode

- **5.1–5.4, 5.6–5.8:** All items complete.

- **5.5:** Implement a dialog to capture "User Notes" before saving.  
  **Status:** 🟡 In Progress  
  **Notes:** ViewModel exists, but `SaveSnapshotDialog.xaml` UI is missing.

---

## Phase 6: Final Polish & Release Readiness

- **6.1:** Implement "Export to CSV" functionality from the `DataGrid`'s context menu.  
  **Status:** ⏳ Pending

- **6.2:** Implement a "Recent Files" list on the `WelcomeView`.  
  **Status:** 🟡 In Progress  
  **Notes:** Recent files property exists in `UserUiSettingsService`, but not integrated in WelcomeView UI.

- **6.3:** Configure file type association for a custom extension (e.g., `.janus`) to open the app.  
  **Status:** 🟡 In Progress  
  **Notes:** File dialogs use `.janus` filter, but no OS-level association.

- **6.4:** Conduct end-to-end testing: scan, save, close, re-open from file.  
  **Status:** ⏳ Pending

- **6.5:** Perform stress testing with large time windows and noisy event logs.  
  **Status:** ⏳ Pending

- **6.6:** Review and refine all UI text, tooltips, and error messages for clarity.  
  **Status:** 🟡 In Progress  
  **Notes:** Some messages/tooltips exist, but no comprehensive review found.

- **6.7:** Add a simple "About" dialog with version and project information.  
  **Status:** ✅ Complete

- **6.8:** Prepare the final release build.  
  **Status:** ⏳ Pending

---

**Legend:**  
✅ Complete 🟡 In Progress ⏳ Pending
