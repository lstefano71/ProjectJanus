using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
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
    private EventLogEntryDisplay? selectedEvent;
    public EventLogEntryDisplay? SelectedEvent
    {
        get => selectedEvent;
        set { selectedEvent = value; OnPropertyChanged(); }
    }
    private bool filterError = true;
    public bool FilterError
    {
        get => filterError;
        set { filterError = value; ApplyFilter(); OnPropertyChanged(); }
    }
    private bool filterWarning = true;
    public bool FilterWarning
    {
        get => filterWarning;
        set { filterWarning = value; ApplyFilter(); OnPropertyChanged(); }
    }
    private bool filterInfo = true;
    public bool FilterInfo
    {
        get => filterInfo;
        set { filterInfo = value; ApplyFilter(); OnPropertyChanged(); }
    }
    private string searchText = string.Empty;
    public string SearchText
    {
        get => searchText;
        set { searchText = value; ApplyFilter(); OnPropertyChanged(); }
    }
    public int EventCount => FilteredEvents.Count;
    public ICommand CopyMessageCommand { get; }
    public ICommand SaveSnapshotCommand { get; }
    public ScanSession? Metadata { get; private set; }
    public string MetadataTimestampLocal => Metadata?.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "";
    public string MetadataSnapshotCreatedLocal => Metadata?.SnapshotCreated.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "";

    public ResultsViewModel()
    {
        CopyMessageCommand = new RelayCommand(_ => CopyMessage(), _ => SelectedEvent is not null);
        SaveSnapshotCommand = new RelayCommand(_ => SaveSnapshot());
    }

    public void LoadEvents(IEnumerable<EventLogEntry> events)
    {
        Events.Clear();
        foreach (var e in events)
            Events.Add(new EventLogEntryDisplay(e));
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredEvents.Clear();
        foreach (var e in Events.Where(FilterPredicate))
            FilteredEvents.Add(e);
        OnPropertyChanged(nameof(EventCount));
    }

    private bool FilterPredicate(EventLogEntryDisplay e)
    {
        if (!FilterError && e.Level == "Error") return false;
        if (!FilterWarning && e.Level == "Warning") return false;
        if (!FilterInfo && e.Level == "Info") return false;
        if (!string.IsNullOrWhiteSpace(SearchText) && !(e.Message?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)) return false;
        return true;
    }

    private void CopyMessage()
    {
        if (SelectedEvent is not null)
            System.Windows.Clipboard.SetText(SelectedEvent.Message ?? string.Empty);
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
        var vm = new SaveSnapshotDialogViewModel();
        dialog.DataContext = vm;
        if (dialog.ShowDialog() == true)
        {
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
