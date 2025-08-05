### **Product Requirements Document: Project Janus (Part 1 of 2)**

*   **Project Name:** Project Janus
*   **Version:** 1.0
*   **Status:** Scoping & Design
*   **Date:** August 5, 2025
*   **Author:** Gemini Solutions Architect

---

### **1.0 Introduction & Vision**

Diagnosing transient issues on Windows machines is a time-consuming and frustrating process. Support engineers, developers, and system administrators spend countless hours manually cross-referencing timestamps across dozens of disparate event logs in the Windows Event Viewer to build a narrative around a failure.

**Project Janus** is a desktop diagnostic utility designed to fundamentally solve this problem. Its vision is to provide a powerful, intuitive, and immediate way to understand the state of a system around a critical moment in time.

Like the two-faced Roman god of time, Janus looks at the events leading up to a moment (the past) and the events immediately following it (the future). By providing a single, consolidated, and chronological view of all system and application events within a configurable time window, Janus transforms hours of manual labor into seconds of automated analysis, enabling faster root cause identification and issue resolution.

Furthermore, Janus is designed for collaboration and offline analysis by allowing these "time snapshots" to be saved into portable, self-contained files.

### **2.0 Goals & Objectives**

*   **User Goal:** Drastically reduce the time and effort required to investigate the root cause of a software crash, system freeze, or service failure.
*   **User Goal:** Empower users to easily share a comprehensive diagnostic snapshot with colleagues or support teams without requiring remote access to the affected machine.
*   **Project Goal:** Create a reliable, performant, and intuitive tool that becomes a standard part of any Windows developer's or administrator's toolkit.
*   **Project Goal:** Ensure the core analysis can be performed with zero dependencies on the source machine once a snapshot is created.

### **3.0 User Personas**

1.  **Devon, the Developer:** Devon works on a complex desktop application. When a user reports a crash, he receives a timestamp. Devon needs to understand what else was happening on the user's system at that exact moment. Was a driver updating? Did another service fail? He needs a clear picture to distinguish his application's bugs from environmental factors.
2.  **Alex, the System Administrator:** Alex manages a fleet of Windows Servers. A critical background service failed overnight. Alex has the timestamp from the failure alert. He needs to see if the failure was preceded by security-related events, hardware warnings, or other application errors on the server. He needs to save his findings to document the incident for an outage report.
3.  **Paula, the Power User:** Paula is a tech-savvy user who experiences intermittent system freezes while gaming. She wants to be able to pinpoint the exact moment of a freeze, run a scan, and see if there is a recurring pattern of driver errors or application faults that she can investigate or report to the hardware vendor.

### **4.0 Feature Set for v1.0**

This section details the functional requirements for the initial release of Project Janus.

#### **F-1.0: Core Application & Environment**

| ID | Feature | Description | Acceptance Criteria |
| :-- | :--- | :--- | :--- |
| **F-1.1** | **Mandatory Admin Elevation** | The application requires administrative privileges to access all system event logs. This must be handled gracefully. | 1. The application manifest must be configured with `requestedExecutionLevel="requireAdministrator"`.<br>2. When a standard user attempts to launch the executable, a standard Windows UAC prompt must be displayed.<br>3. If the user denies the UAC prompt, the application does not launch.<br>4. If the user accepts the UAC prompt, the application launches with administrative privileges. |
| **F-1.2** | **Application Welcome Screen** | On launch, the user is presented with a clear choice of what to do. | 1. The screen displays two primary, prominent options: "New Live Scan" and "Open Snapshot...".<br>2. A list of recently opened snapshot files is displayed and is clickable.<br>3. The welcome screen is bypassed if the application is launched by double-clicking a snapshot file. |
| **F-1.3** | **Snapshot File Association** | Snapshot files should be associated with the Janus application for easy opening. | 1. A unique file extension (e.g., `.janus`) is used for snapshot files.<br>2. Installing the application (if an installer is created) or a first-run option registers this file type with Windows Explorer.<br>3. Double-clicking a `.janus` file launches Project Janus and immediately loads that snapshot for offline analysis. |

#### **F-2.0: Live System Analysis**

| ID | Feature | Description | Acceptance Criteria |
| :-- | :--- | :--- | :--- |
| **F-2.1** | **Timestamp & Window Input** | The user must be able to specify the central point-in-time and the surrounding time window for the scan. | 1. The UI provides separate controls for selecting the date and the time of the central event.<br>2. A "Now" button is present, which, when clicked, populates the date and time controls with the current system time.<br>3. The UI provides two separate controls (e.g., numeric up/down) to configure "Minutes Before" and "Minutes After".<br>4. Default values are provided for the time window (e.g., 5 minutes before, 5 minutes after). |
| **F-2.2**| **Asynchronous Scan Process** | The scan of all event logs must not freeze the application UI. The user must receive continuous feedback and be able to cancel the operation. | 1. Clicking the "Scan" button initiates the scan on a background thread.<br>2. While scanning, the "Scan" button is disabled and a "Cancel" button becomes visible and enabled.<br>3. A status bar/label displays the name of the log currently being scanned (e.g., "Scanning: System").<br>4. A running counter of total events found is updated in real-time on the UI.<br>5. Clicking the "Cancel" button cleanly aborts the scan. The UI becomes responsive, and the results collected so far are displayed. |
| **F-2.3** | **Comprehensive Log Discovery** | The scan must include all available event logs on the system, including application-specific and diagnostic logs. | 1. The application queries `EventLogSession.GlobalSession` to retrieve a list of all registered event log providers.<br>2. The scan iterates over this entire list.<br>3. The scan logic must gracefully handle and log any `EventLogException` that occurs when trying to access a specific log (e.g., if it's corrupted), without halting the entire scan process. A summary of failed logs can be presented upon completion. |

---
Of course. Here is the second and final part of the Product Requirements Document for Project Janus.

---

### **Product Requirements Document: Project Janus (Part 2 of 2)**

*   **Project Name:** Project Janus
*   **Version:** 1.0
*   **Status:** Scoping & Design
*   **Date:** August 5, 2025
*   **Author:** Gemini Solutions Architect

---

#### **F-3.0: Results Display & Interaction**

This section defines how the collected event data, from either a live scan or a loaded snapshot, is presented to the user.

| ID | Feature | Description | Acceptance Criteria |
| :-- | :--- | :--- | :--- |
| **F-3.1** | **Event Results Grid** | A primary grid view must display the list of all found events in a clear, sortable, and readable format. | 1. The view must be a data grid with UI virtualization enabled to handle large result sets efficiently.<br>2. The grid must be sorted chronologically by timestamp by default.<br>3. The following columns must be visible by default: Timestamp (in user's local time), Level (Error, Warning, etc.), Source (Provider Name), Event ID, Log Name.<br>4. The user must be able to sort the results by clicking on any column header. |
| **F-3.2** | **Visual Level Highlighting** | Event entries in the grid must be color-coded to allow for at-a-glance identification of severity. | 1. Rows corresponding to "Error" or "Critical" level events must have a distinct red-toned background.<br>2. Rows corresponding to "Warning" level events must have a distinct yellow-toned background.<br>3. All other event levels (Information, Verbose) will use the default background color. |
| **F-3.3** | **Event Detail View** | Selecting an event in the grid must display its full, detailed message. | 1. A separate, read-only text area is present in the UI.<br>2. When a user selects a single row in the results grid, this text area is populated with the fully formatted event message (`EventRecord.FormatDescription()`).<br>3. If multiple events are selected, the detail view is cleared or shows a message like "Select a single event to see details." |
| **F-3.4** | **Result Filtering & Search** | The user must be able to quickly filter and search the displayed results to narrow down the data. | 1. The UI provides checkboxes or toggle buttons to filter by Level (Error, Warning, Information).<br>2. A text search box is available. As the user types, the grid is filtered to show only events where the search term appears in any of the displayed fields or the detailed message.<br>3. Filtering and searching must work on both live scan results and data loaded from a snapshot. |
| **F-3.5** | **Data Exporting** | Users must be able to export data for use in other tools or reports. | 1. A right-click context menu on the results grid provides an "Export" option.<br>2. Sub-options include "Export selected rows to CSV" and "Export all rows to CSV".<br>3. Another context menu option, "Copy Message," copies the full detail message of the selected event to the clipboard. |

#### **F-4.0: Snapshot Management (Offline Analysis)**

This section defines the functionality for saving and loading scan results, which is a core tenet of the project.

| ID | Feature | Description | Acceptance Criteria |
| :-- | :--- | :--- | :--- |
| **F-4.1** | **Save Snapshot** | After a live scan is complete, the user must be able to save the entire session into a single, portable file. | 1. A "Save Snapshot" button is available on the results screen of a live scan.<br>2. Clicking it opens a standard "Save File" dialog, pre-configured for the `.janus` file extension.<br>3. Before saving, a dialog prompts the user for optional "User Notes" which can be multiline.<br>4. All captured event data, the scan parameters (center timestamp, window), the machine name, the scan timestamp, and the user notes are saved into the specified SQLite database file. |
| **F-4.2** | **Portable Data Capture** | The snapshot file must be self-contained and require no external resources from the source machine to be analyzed. | 1. During the initial scan, the fully formatted event message string (`FormatDescription()`) is retrieved and stored in the database.<br>2. The resulting `.janus` file has no dependencies on the provider DLLs of the machine where the scan was performed. |
| **F-4.3** | **Load Snapshot** | A user must be able to open a `.janus` file to view its contents within the application. | 1. The "Open Snapshot..." button on the Welcome Screen opens a standard "Open File" dialog filtered for `.janus` files.<br>2. Upon opening, the application loads all data from the SQLite file.<br>3. The application displays the results in the same grid view (F-3.1) used for live scans.<br>4. The UI must clearly indicate that it is in "Offline Mode." |
| **F-4.4** | **Display Snapshot Metadata** | When viewing a snapshot, the user must see the context in which it was created. | 1. A prominent, read-only area of the UI displays the metadata from the `ScanSession` record.<br>2. Displayed fields must include: Machine Name, Scan Timestamp, Center Timestamp, Time Window, and the User Notes. |
| **F-4.5**| **Schema Version Handling** | The application must gracefully handle opening snapshots created with an incompatible version of the application. | 1. The application will not use a formal migration system. It will use `EnsureCreated` for new files.<br>2. When attempting to open a `.janus` file, if an `SqliteException` occurs due to a schema mismatch (e.g., "no such column"), the read operation is aborted.<br>3. A user-friendly message box is displayed, informing the user that the file version is incompatible with the current version of Project Janus. |

### **5.0 Technical Architecture & Design**

*   **Platform & Language:** .NET 9, C# 13.
*   **Application Framework:** Windows Presentation Foundation (WPF) or WinUI 3, to be selected for its rich data binding and UI customization capabilities. The Model-View-ViewModel (MVVM) pattern will be strictly followed.
*   **Event Log Access:** The modern `System.Diagnostics.Eventing.Reader` namespace will be used for all event log interactions due to its performance and comprehensive querying capabilities via XPath.
*   **Persistence Layer:**
    *   **Database:** SQLite will be used as the embedded database engine.
    *   **Data Access:** Entity Framework Core (EF Core) will be used as the ORM to interact with the SQLite database.
    *   **Schema Management:** The `DbContext.Database.EnsureCreatedAsync()` method will be used to generate the database schema on-the-fly for new snapshot files. **Formal EF Core Migrations will not be used** to maintain simplicity and align with the "document database" model of the snapshots.
*   **Data Models:** The core models in `Janus.Core` will be `ScanSession` and `EventLogEntry`, representing the saved snapshot and individual events, respectively.
*   **Concurrency:** Heavy use of `async`/`await`, `Task`, `IAsyncEnumerable<T>`, and `CancellationToken` will be made to ensure a responsive UI and efficient, cancellable background processing.

### **6.0 Non-Functional Requirements**

| ID | Requirement | Description |
| :-- | :--- | :--- |
| **NFR-1** | **Performance** | The UI must remain responsive at all times. Scans of a +/- 10-minute window on a typical system should complete in a reasonable amount of time (e.g., under 30-60 seconds). Loading and filtering a snapshot with up to 100,000 events should be near-instantaneous. |
| **NFR-2** | **Usability** | The interface must be clean, uncluttered, and highly task-focused. A new user should be able to perform a live scan and save a snapshot with minimal guidance. |
| **NFR-3** | **Reliability** | The application must not crash due to external factors like corrupted event logs or malformed snapshot files. All file I/O and event log access must be wrapped in appropriate error handling. |
| **NFR-4** | **Portability** | The generated `.janus` snapshot file must be 100% self-contained and have no runtime dependencies on the machine it was created on. |

### **7.0 Future Considerations (Post v1.0)**

The following features are explicitly out of scope for version 1.0 but represent potential future directions for the project:

*   **Snapshot Comparison ("Diffing"):** A feature to load two snapshots and highlight the events that are unique to one of them.
*   **Advanced Report Generation:** Creating formatted HTML or PDF reports from a snapshot.
*   **Real-time Monitoring:** A mode to watch event logs in real-time and flag events matching certain criteria as they occur.

---
