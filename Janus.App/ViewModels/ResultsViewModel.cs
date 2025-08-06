using Janus.App.Services;
using Janus.Core;

using Microsoft.Win32;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Janus.App;

public class EventLogEntryDisplay : INotifyPropertyChanged
{
  private readonly EventLogEntry entry;

  public EventLogEntryDisplay(EventLogEntry entry)
  {
    this.entry = entry;
  }

  public int ScanEventId => entry.ScanEventId;
  public long Id => entry.Id;
  public string LogName => entry.LogName;
  public DateTime TimeCreated => entry.TimeCreated;
  public string Level => entry.Level;
  public string Source => entry.Source;
  public int EventId => entry.EventId;
  public string Message => entry.Message;
  public string MachineName => entry.MachineName;
  public Guid ScanSessionId => entry.ScanSessionId;
  // Display ISO 8601 format with microseconds (6 digits)
  public string TimeCreatedIso => entry.TimeCreated.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'");

  public event PropertyChangedEventHandler? PropertyChanged;
}

public partial class ResultsViewModel : INotifyPropertyChanged
{
  public enum PreviousView { Welcome, LiveScan }

  private readonly UserUiSettingsService uiSettingsService = UserUiSettingsService.Instance;
  private readonly System.Timers.Timer debounceTimer;
  private bool pendingSave;

  private double resultsSplitterPosition = 0.5;
  public double ResultsSplitterPosition {
    get => resultsSplitterPosition;
    set {
      if (resultsSplitterPosition != value) {
        resultsSplitterPosition = value;
        OnPropertyChanged();
        DebounceSave();
      }
    }
  }

  private double detailsSplitterPosition = 0.33;
  public double DetailsSplitterPosition {
    get => detailsSplitterPosition;
    set {
      if (detailsSplitterPosition != value) {
        detailsSplitterPosition = value;
        OnPropertyChanged();
        DebounceSave();
      }
    }
  }

  public ObservableCollection<EventLogEntryDisplay> Events { get; } = [];
  private ICollectionView _eventsView;
  public ICollectionView EventsView {
    get => _eventsView;
    set { _eventsView = value; OnPropertyChanged(); }
  }
  public bool HasEvents => EventCount > 0;

  private int eventCount;
  public int EventCount {
    get => eventCount;
    set {
      if (eventCount != value) {
        eventCount = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(EventCountStatus)); // Update status string when count changes
                                                     // Crucially, notify that HasEvents has also changed
        OnPropertyChanged(nameof(HasEvents));
      }
    }
  }
  public int TotalEventCount => Events.Count;
  public string EventCountStatus => $"{EventCount} / {TotalEventCount} events";

  // Log level filter options
  public ObservableCollection<string> LogLevels { get; } = ["Critical", "Error", "Warning", "Information", "Verbose"];
  private ObservableCollection<string> selectedLogLevels = [];
  public ObservableCollection<string> SelectedLogLevels {
    get => selectedLogLevels;
    set {
      selectedLogLevels = value;
      OnPropertyChanged();
      OnPropertyChanged(nameof(SelectedLogLevelsDisplay));
      OnPropertyChanged(nameof(EventCountStatus));
      EventsView.Refresh();
    }
  }
  public string SelectedLogLevelsDisplay => SelectedLogLevels.Count == 0 ? "All Levels" : string.Join(", ", SelectedLogLevels);
  private bool isLogLevelPopupOpen;
  public bool IsLogLevelPopupOpen {
    get => isLogLevelPopupOpen;
    set { isLogLevelPopupOpen = value; OnPropertyChanged(); }
  }
  public ICommand ToggleLogLevelCommand { get; }
  private void ToggleLogLevel(object? level)
  {
    if (level is string logLevel) {
      if (!SelectedLogLevels.Remove(logLevel))
        SelectedLogLevels.Add(logLevel);
      OnPropertyChanged(nameof(SelectedLogLevelsDisplay));
      RebuildView(); // Call the new rebuild method

    }
  }

  // Grouping toggle
  private bool isGroupingEnabled;
  public bool IsGroupingEnabled {
    get => isGroupingEnabled;
    set {
      isGroupingEnabled = value; OnPropertyChanged(); UpdateGrouping();
      OnPropertyChanged(nameof(IsAnyGroupingEnabled)); // Notify combined property
    }
  }
  private bool isGroupByLevelEnabled;
  public bool IsGroupByLevelEnabled {
    get => isGroupByLevelEnabled;
    set {
      if (isGroupByLevelEnabled != value) {
        isGroupByLevelEnabled = value;
        // if (value) IsGroupingEnabled = false; // mutually exclusive
        OnPropertyChanged();
        UpdateGrouping();
        OnPropertyChanged(nameof(IsAnyGroupingEnabled)); // Notify combined property

      }
    }
  }
  // The NEW combined property for the View to use.
  public bool IsAnyGroupingEnabled => IsGroupingEnabled || IsGroupByLevelEnabled;

  // This method is now correct and necessary. It manages the DATA, not the visuals.
  private void UpdateGrouping()
  {
    EventsView.GroupDescriptions.Clear();
    if (IsGroupingEnabled)
      EventsView.GroupDescriptions.Add(new PropertyGroupDescription("LogName"));
    else if (IsGroupByLevelEnabled)
      EventsView.GroupDescriptions.Add(new PropertyGroupDescription("Level"));
  }

  // Progress/Status
  private string scanStatus = "Ready";
  public string ScanStatus {
    get => scanStatus;
    set { scanStatus = value; OnPropertyChanged(); }
  }

  private EventLogEntryDisplay? selectedEvent;
  public EventLogEntryDisplay? SelectedEvent {
    get => selectedEvent;
    set { selectedEvent = value; OnPropertyChanged(); }
  }

  private string searchText = string.Empty;
  public string SearchText {
    get => searchText;
    set {
      searchText = value;
      OnPropertyChanged();
      RebuildView(); // Call the new rebuild method
    }
  }
  private void RebuildView()
  {
    // 1. Create a completely new CollectionViewSource.
    var cvs = new CollectionViewSource { Source = Events };

    // 2. Re-apply the filter to the new view.
    cvs.Filter += (s, e) => {
      e.Accepted = FilterPredicate(e.Item as EventLogEntryDisplay);
    };

    // 3. Set the new view to our property. This triggers the UI to update.
    // The DataGrid will see it's a NEW object and completely reset itself.
    EventsView = cvs.View;

    // 3.5. Always sort by TimeCreated descending (most recent first)
    EventsView.SortDescriptions.Clear();
    EventsView.SortDescriptions.Add(new SortDescription(nameof(EventLogEntryDisplay.TimeCreated), ListSortDirection.Ascending));

    // 4. Re-apply grouping and update counts on the new view.
    UpdateGrouping();
    EventCount = EventsView.Cast<object>().Count();
  }
  public ICommand CopyMessageCommand { get; }
  public ICommand SaveSnapshotCommand { get; }
  public ICommand BackCommand { get; }
  public ScanSession? Metadata { get; private set; }
  public string MetadataTimestampLocal => Metadata?.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "";
  public string MetadataSnapshotCreatedLocal => Metadata?.SnapshotCreated.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "";

  private readonly Action<object>? setCurrentView;
  private readonly PreviousView previousView;

  private bool canSaveSnapshot = true;
  public bool CanSaveSnapshot {
    get => canSaveSnapshot;
    set {
      if (canSaveSnapshot != value) {
        canSaveSnapshot = value;
        OnPropertyChanged();
        (SaveSnapshotCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
      }
    }
  }

  // Add a HashSet to track checked ScanEventIds
  private readonly HashSet<int> checkedScanEventIds = [];
  public IReadOnlyCollection<int> CheckedScanEventIds => checkedScanEventIds;

  // Add the ToggleCheckedCommand property
  public ICommand ToggleCheckedCommand { get; }

  // Add a property to control filtering for checked rows
  private bool showOnlyChecked;
  public bool ShowOnlyChecked {
    get => showOnlyChecked;
    set {
      if (showOnlyChecked != value) {
        showOnlyChecked = value;
        OnPropertyChanged();
        RebuildView();
      }
    }
  }

  // Helper property: true if there is at least one checked row
  public bool HasCheckedRows => checkedScanEventIds.Count > 0;

  public ResultsViewModel(Action<object>? setCurrentView = null, PreviousView previousView = PreviousView.Welcome)
  {
    this.setCurrentView = setCurrentView;
    this.previousView = previousView;

    RebuildView();

    CopyMessageCommand = new RelayCommand(_ => CopyMessage(), _ => SelectedEvent is not null);
    SaveSnapshotCommand = new AsyncRelayCommand(SaveSnapshotAsync, () => CanSaveSnapshot);
    ToggleLogLevelCommand = new RelayCommand(ToggleLogLevel);
    BackCommand = new RelayCommand(_ => GoBack(), _ => setCurrentView != null);
    ToggleCheckedCommand = new RelayCommand(ToggleChecked);
    debounceTimer = new System.Timers.Timer(500) { AutoReset = false };
    debounceTimer.Elapsed += async (_, __) => {
      if (pendingSave) {
        await SaveUiSettingsAsync();
        pendingSave = false;
      }
    };
    _ = LoadUiSettingsAsync();
  }

  // Toggle checked state for a row (parameter is EventLogEntryDisplay)
  private void ToggleChecked(object? param)
  {
    if (param is EventLogEntryDisplay entry) {
      if (!checkedScanEventIds.Add(entry.ScanEventId))
        checkedScanEventIds.Remove(entry.ScanEventId);
      OnPropertyChanged(nameof(CheckedScanEventIds));
      OnPropertyChanged(nameof(HasCheckedRows));
      if (ShowOnlyChecked) RebuildView();
    }
  }

  private void GoBack()
  {
    if (setCurrentView == null) return;
    if (previousView == PreviousView.LiveScan)
      setCurrentView(new LiveScanViewModel(setCurrentView));
    else
      setCurrentView(new WelcomeViewModel(setCurrentView));
  }

  public void LoadEvents(IEnumerable<EventLogEntry> events)
  {
    Events.Clear();
    foreach (var e in events) {
      Events.Add(new EventLogEntryDisplay(e));
      if (!LogLevels.Contains(e.Level))
        LogLevels.Add(e.Level);
    }
    EventsView.Refresh();
    UpdateGrouping();
    ScanStatus = $"Loaded {Events.Count} events.";
    RebuildView();
  }

  private bool FilterPredicate(EventLogEntryDisplay? e)
  {
    if (e == null) return false;
    // If ShowOnlyChecked is enabled and there are checked rows, only show checked
    if (ShowOnlyChecked && HasCheckedRows && !checkedScanEventIds.Contains(e.ScanEventId)) return false;
    if (SelectedLogLevels.Count > 0 && !SelectedLogLevels.Contains(e.Level)) return false;
    if (!string.IsNullOrWhiteSpace(SearchText) && !(e.Message?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)) return false;
    return true;
  }

  private void CopyMessage()
  {
    if (SelectedEvent is not null)
      Clipboard.SetText(SelectedEvent.Message ?? string.Empty);
  }

  public void SetMetadata(ScanSession session)
  {
    Metadata = session;
    OnPropertyChanged(nameof(Metadata));
    OnPropertyChanged(nameof(MetadataTimestampLocal));
    OnPropertyChanged(nameof(MetadataSnapshotCreatedLocal));
  }

  private async Task SaveSnapshotAsync()
  {
    var dialog = new SaveSnapshotDialog();
    if (dialog.ShowDialog() == true) {
      var vm = (SaveSnapshotDialogViewModel)dialog.DataContext;
      string? defaultFileName = null;
      if (Metadata != null) {
        // Use ISO 8601 format for the pivot timestamp (Metadata.Timestamp)
        // Replace characters not allowed in file names (e.g., ':') with '-'
        var iso = Metadata.Timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH-mm-ssZ");
        defaultFileName = $"JanusSnapshot_{iso}.janus";
      }
      var saveFileDialog = new SaveFileDialog {
        Filter = "Janus Snapshot (*.janus)|*.janus",
        DefaultExt = "janus",
        FileName = defaultFileName
      };
      if (saveFileDialog.ShowDialog() == true && Metadata is not null) {
        var filePath = saveFileDialog.FileName;
        if (File.Exists(filePath)) {
          try {
            File.Delete(filePath);
          } catch (Exception ex) {
            MessageBox.Show($"Failed to delete existing file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
          }
        }
        var updatedSession = new ScanSession {
          Id = Metadata.Id,
          Timestamp = Metadata.Timestamp,
          MinutesBefore = Metadata.MinutesBefore,
          MinutesAfter = Metadata.MinutesAfter,
          SnapshotCreated = DateTime.UtcNow,
          MachineName = Metadata.MachineName,
          UserNotes = vm.UserNotes,
          Entries = Metadata.Entries
        };
        var service = new SnapshotService();
        await SnapshotService.SaveSnapshotAsync(filePath, updatedSession);
        await UserUiSettingsService.AddRecentSnapshotAsync(filePath);
        MessageBox.Show($"Snapshot saved successfully at: {filePath}.",
          "Success", MessageBoxButton.OK, MessageBoxImage.Information);

        SetMetadata(updatedSession);
        CanSaveSnapshot = false;
      }
    }
  }

  private async Task LoadUiSettingsAsync()
  {
    var settings = await UserUiSettingsService.LoadAsync();
    ResultsSplitterPosition = settings.ResultsSplitterPosition;
    DetailsSplitterPosition = settings.DetailsSplitterPosition;
  }

  private async Task SaveUiSettingsAsync()
  {
    var settings = await UserUiSettingsService.LoadAsync();
    settings.ResultsSplitterPosition = ResultsSplitterPosition;
    settings.DetailsSplitterPosition = DetailsSplitterPosition;
    await UserUiSettingsService.SaveAsync(settings);
  }

  private void DebounceSave()
  {
    pendingSave = true;
    debounceTimer.Stop();
    debounceTimer.Start();
  }

  public event PropertyChangedEventHandler? PropertyChanged;
  private void OnPropertyChanged([CallerMemberName] string? name = null)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
