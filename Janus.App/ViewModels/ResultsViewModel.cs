using Janus.App.Services;
using Janus.Core;

using Microsoft.Win32;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Janus.App;

public class EventLogEntryDisplay
{
  private readonly EventLogEntry entry;
  public EventLogEntryDisplay(EventLogEntry entry)
  {
    this.entry = entry;
  }
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

  public ObservableCollection<EventLogEntryDisplay> Events { get; } = new();
  public ICollectionView EventsView { get; }

  // Log level filter options
  public ObservableCollection<string> LogLevels { get; } = new() { "Critical", "Error", "Warning", "Information", "Verbose" };
  private ObservableCollection<string> selectedLogLevels = new();
  public ObservableCollection<string> SelectedLogLevels {
    get => selectedLogLevels;
    set {
      selectedLogLevels = value;
      OnPropertyChanged();
      OnPropertyChanged(nameof(SelectedLogLevelsDisplay));
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
      if (SelectedLogLevels.Contains(logLevel))
        SelectedLogLevels.Remove(logLevel);
      else
        SelectedLogLevels.Add(logLevel);
      OnPropertyChanged(nameof(SelectedLogLevelsDisplay));
      EventsView.Refresh();
    }
  }

  // Grouping toggle
  private bool isGroupingEnabled;
  public bool IsGroupingEnabled {
    get => isGroupingEnabled;
    set { isGroupingEnabled = value; OnPropertyChanged(); UpdateGrouping(); }
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
      }
    }
  }
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
    set { searchText = value; EventsView.Refresh(); OnPropertyChanged(); }
  }

  public int EventCount => EventsView.Cast<object>().Count();
  public ICommand CopyMessageCommand { get; }
  public ICommand SaveSnapshotCommand { get; }
  public ICommand BackCommand { get; }
  public ScanSession? Metadata { get; private set; }
  public string MetadataTimestampLocal => Metadata?.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "";
  public string MetadataSnapshotCreatedLocal => Metadata?.SnapshotCreated.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "";

  private readonly Action<object>? setCurrentView;
  private readonly PreviousView previousView;

  public ResultsViewModel(Action<object>? setCurrentView = null, PreviousView previousView = PreviousView.Welcome)
  {
    this.setCurrentView = setCurrentView;
    this.previousView = previousView;
    EventsView = CollectionViewSource.GetDefaultView(Events);
    EventsView.Filter = o => FilterPredicate(o as EventLogEntryDisplay);
    CopyMessageCommand = new RelayCommand(_ => CopyMessage(), _ => SelectedEvent is not null);
    SaveSnapshotCommand = new RelayCommand(_ => SaveSnapshot());
    ToggleLogLevelCommand = new RelayCommand(ToggleLogLevel);
    BackCommand = new RelayCommand(_ => GoBack(), _ => setCurrentView != null);
    debounceTimer = new System.Timers.Timer(500) { AutoReset = false };
    debounceTimer.Elapsed += async (_, __) => {
      if (pendingSave) {
        await SaveUiSettingsAsync();
        pendingSave = false;
      }
    };
    _ = LoadUiSettingsAsync();
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
  }

  private bool FilterPredicate(EventLogEntryDisplay? e)
  {
    if (e == null) return false;
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

  private void SaveSnapshot()
  {
    var dialog = new SaveSnapshotDialog();
    if (dialog.ShowDialog() == true) {
      var vm = (SaveSnapshotDialogViewModel)dialog.DataContext;
      var saveFileDialog = new SaveFileDialog {
        Filter = "Janus Snapshot (*.janus)|*.janus",
        DefaultExt = "janus"
      };
      if (saveFileDialog.ShowDialog() == true && Metadata is not null) {
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
        service.SaveSnapshotAsync(saveFileDialog.FileName, updatedSession).Wait();
        MessageBox.Show("Snapshot saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        SetMetadata(updatedSession);
      }
    }
  }

  private async Task LoadUiSettingsAsync()
  {
    var settings = await uiSettingsService.LoadAsync();
    ResultsSplitterPosition = settings.ResultsSplitterPosition;
    DetailsSplitterPosition = settings.DetailsSplitterPosition;
  }

  private async Task SaveUiSettingsAsync()
  {
    var settings = await uiSettingsService.LoadAsync();
    settings.ResultsSplitterPosition = ResultsSplitterPosition;
    settings.DetailsSplitterPosition = DetailsSplitterPosition;
    await uiSettingsService.SaveAsync(settings);
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
