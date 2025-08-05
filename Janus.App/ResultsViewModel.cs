using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using Janus.Core;
using Microsoft.Win32;

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
    public string TimeCreatedLocal => entry.TimeCreated.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
}

public class ResultsViewModel : INotifyPropertyChanged
{
    public ObservableCollection<EventLogEntryDisplay> Events { get; } = new();
    public ObservableCollection<EventLogEntryDisplay> FilteredEvents { get; } = new();
    public ICollectionView EventsView { get; }

    // Log level filter options
    public ObservableCollection<string> LogLevels { get; } = new() { "Critical", "Error", "Warning", "Information", "Verbose" };
    private ObservableCollection<string> selectedLogLevels = new();
    public ObservableCollection<string> SelectedLogLevels
    {
        get => selectedLogLevels;
        set
        {
            selectedLogLevels = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedLogLevelsDisplay));
            EventsView.Refresh();
        }
    }
    public string SelectedLogLevelsDisplay => SelectedLogLevels.Count == 0 ? "All Levels" : string.Join(", ", SelectedLogLevels);
    private bool isLogLevelPopupOpen;
    public bool IsLogLevelPopupOpen
    {
        get => isLogLevelPopupOpen;
        set { isLogLevelPopupOpen = value; OnPropertyChanged(); }
    }
    public ICommand ToggleLogLevelCommand { get; }
    private void ToggleLogLevel(object? level)
    {
        if (level is string logLevel)
        {
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
    public bool IsGroupingEnabled
    {
        get => isGroupingEnabled;
        set { isGroupingEnabled = value; OnPropertyChanged(); UpdateGrouping(); }
    }
    private void UpdateGrouping()
    {
        EventsView.GroupDescriptions.Clear();
        if (IsGroupingEnabled)
            EventsView.GroupDescriptions.Add(new PropertyGroupDescription("LogName"));
    }

    // Progress/Status
    private string scanStatus = "Ready";
    public string ScanStatus
    {
        get => scanStatus;
        set { scanStatus = value; OnPropertyChanged(); }
    }

    private EventLogEntryDisplay? selectedEvent;
    public EventLogEntryDisplay? SelectedEvent
    {
        get => selectedEvent;
        set { selectedEvent = value; OnPropertyChanged(); }
    }

    private string searchText = string.Empty;
    public string SearchText
    {
        get => searchText;
        set { searchText = value; EventsView.Refresh(); OnPropertyChanged(); }
    }

    public int EventCount => EventsView.Cast<object>().Count();
    public ICommand CopyMessageCommand { get; }
    public ICommand SaveSnapshotCommand { get; }
    public ScanSession? Metadata { get; private set; }
    public string MetadataTimestampLocal => Metadata?.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "";
    public string MetadataSnapshotCreatedLocal => Metadata?.SnapshotCreated.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "";

    public ResultsViewModel()
    {
        EventsView = CollectionViewSource.GetDefaultView(Events);
        EventsView.Filter = o => FilterPredicate(o as EventLogEntryDisplay);
        CopyMessageCommand = new RelayCommand(_ => CopyMessage(), _ => SelectedEvent is not null);
        SaveSnapshotCommand = new RelayCommand(_ => SaveSnapshot());
        ToggleLogLevelCommand = new RelayCommand(ToggleLogLevel);
    }

    public void LoadEvents(IEnumerable<EventLogEntry> events)
    {
        Events.Clear();
        foreach (var e in events)
        {
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
        if (dialog.ShowDialog() == true)
        {
            var vm = (SaveSnapshotDialogViewModel)dialog.DataContext;
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Janus Snapshot (*.janus)|*.janus",
                DefaultExt = "janus"
            };
            if (saveFileDialog.ShowDialog() == true && Metadata is not null)
            {
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

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
