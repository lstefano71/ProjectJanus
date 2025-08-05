
### Phase 1: Project Foundation & Core Data Model

-   [x] **1.1:** Set up solution structure: `ProjectJanus.sln`.
-   [x] **1.2:** Create main application project (WPF or WinUI): `Janus.App`.
-   [x] **1.3:** Create class library for data models: `Janus.Core`.
-   [x] **1.4:** Add NuGet packages to `Janus.Core`: `Microsoft.EntityFrameworkCore.Sqlite`.
-   [x] **1.5:** Define data model class `EventLogEntry.cs` in `Janus.Core`.
-   [x] **1.6:** Define data model class `ScanSession.cs` in `Janus.Core`.
-   [x] **1.7:** Implement EF Core context `EventSnapshotDbContext.cs` in `Janus.Core`.
-   [ ] **1.8:** Add `app.manifest` to `Janus.App` and configure `requestedExecutionLevel` to `requireAdministrator`.
-   [x] **1.9:** Initialize a Git repository and create a standard `.gitignore` file for .NET projects.

### Phase 2: Live Event Scanning Engine

   [x] **2.1:** Create service class `EventLogScannerService.cs` in `Janus.App`.
   [x] **2.2:** Implement method to retrieve all system log names using `EventLogSession`.
   [x] **2.3:** Implement the core `ScanAllLogsAsync` method which accepts timestamp, time window, `IProgress<T>`, and `CancellationToken`.
   [x] **2.4:** Implement XPath query logic within `ScanAllLogsAsync` to filter events by `TimeCreated`.
   [~] **2.5:** Parallelize the querying of individual event logs for performance. // Log queries are processed sequentially, not in parallel.
   [x] **2.6:** Implement robust `try-catch (EventLogException)` for each log query to handle corrupted logs.
   [x] **2.7:** Implement cancellation logic throughout the async scan process using the `CancellationToken`.
   [x] **2.8:** Implement progress reporting for scan status (e.g., "Scanning: System") and total events found.

### Phase 3: UI - Live Scan & Control

-   [-] **3.1:** Design the application's main shell window (`MainWindow.xaml`).
        <!-- MainWindow.xaml.cs exists; shell window class is present. UI design details not confirmed. -->
-   [-] **3.2:** Create a `LiveScanView.xaml` UserControl for the initial scan configuration screen.
        <!-- LiveScanView.xaml not found; not started. -->
-   [-] **3.3:** Add UI controls to `LiveScanView`: `DatePicker`, time control, "Now" button, numeric controls for time window.
        <!-- LiveScanView.xaml not found; controls not implemented. -->
-   [-] **3.4:** Add "Scan" and "Cancel" buttons to `LiveScanView`.
        <!-- LiveScanView.xaml not found; buttons not implemented. -->
-   [-] **3.5:** Add a status bar area to the UI for progress text and event count.
        <!-- MainWindow.xaml.cs exists; status bar implementation not confirmed. -->
-   [-] **3.6:** Create `LiveScanViewModel.cs` and bind it to `LiveScanView`.
        <!-- LiveScanViewModel.cs not found; not started. -->
-   [-] **3.7:** Implement the "Scan" command in the ViewModel to invoke the `EventLogScannerService`.
        <!-- MainViewModel.cs contains RelayCommand methods; specific Scan command implementation not confirmed. -->
-   [-] **3.8:** Implement the "Cancel" command to trigger the `CancellationTokenSource`.
        <!-- MainViewModel.cs contains CanCancelScan logic; full Cancel command implementation not confirmed. -->
-   [-] **3.9:** Implement the "Now" button logic to populate the timestamp controls with the current system time.
        <!-- "Now" button logic not found; not implemented. -->

### Phase 4: UI - Results Display & Interaction

-   [ ] **4.1:** Create a `ResultsView.xaml` UserControl to display scan results.
-   [ ] **4.2:** Add and configure a `DataGrid` in `ResultsView` for the event list (Timestamp, Level, Source, ID, Log Name).
-   [ ] **4.3:** Implement `DataGrid` row styling with data triggers to color-code rows based on event level (Error, Warning).
-   [ ] **4.4:** Add a read-only `TextBox` to `ResultsView` for displaying the full message of the selected event.
-   [ ] **4.5:** Create a `ResultsViewModel.cs` to manage the result data and presentation logic.
-   [ ] **4.6:** Bind the `DataGrid`'s `ItemsSource` to an `ObservableCollection` in the ViewModel.
-   [ ] **4.7:** Implement logic to update the detail view when the `DataGrid`'s selected item changes.
-   [ ] **4.8:** Add UI controls for filtering: checkboxes for Level and a search `TextBox`.
-   [ ] **4.9:** Implement filtering logic in the ViewModel, preferably using `ICollectionView` for efficiency.
-   [ ] **4.10:** Add a right-click context menu to the `DataGrid` with a "Copy Message" command.

### Phase 5: Snapshot Persistence & Offline Mode

-   [ ] **5.1:** Create service class `SnapshotService.cs` in `Janus.App`.
-   [ ] **5.2:** Implement `SaveSnapshotAsync` method using `EventSnapshotDbContext` and `Database.EnsureCreatedAsync()`.
-   [ ] **5.3:** Implement `LoadSnapshotAsync` method, including `try-catch` for `SqliteException` to handle schema mismatches.
-   [ ] **5.4:** Add a "Save Snapshot" button to the results view, wired to a ViewModel command.
-   [ ] **5.5:** Implement a dialog to capture "User Notes" before saving.
-   [ ] **5.6:** Create a `WelcomeView.xaml` UserControl with "New Live Scan" and "Open Snapshot..." options.
-   [ ] **5.7:** Implement the "Open Snapshot" command to use `SnapshotService` and navigate to the results view.
-   [ ] **5.8:** Add a read-only metadata display area to the `ResultsView` (for machine name, notes, etc.), visible only when viewing a snapshot.

### Phase 6: Final Polish & Release Readiness

-   [ ] **6.1:** Implement "Export to CSV" functionality from the `DataGrid`'s context menu.
-   [ ] **6.2:** Implement a "Recent Files" list on the `WelcomeView`.
-   [ ] **6.3:** Configure file type association for a custom extension (e.g., `.janus`) to open the app.
-   [ ] **6.4:** Conduct end-to-end testing: scan, save, close, re-open from file.
-   [ ] **6.5:** Perform stress testing with large time windows and noisy event logs.
-   [ ] **6.6:** Review and refine all UI text, tooltips, and error messages for clarity.
-   [ ] **6.7:** Add a simple "About" dialog with version and project information.
-   [ ] **6.8:** Prepare the final release build.
