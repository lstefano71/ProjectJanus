using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Janus.Core;

using System.Windows.Data;

namespace Janus.App;

using System.Collections.ObjectModel;

public class LiveScanViewModel : INotifyPropertyChanged
{
    // For ComboBox multi-select popup
    private bool isLogLevelPopupOpen;
    public bool IsLogLevelPopupOpen
    {
        get => isLogLevelPopupOpen;
        set { isLogLevelPopupOpen = value; OnPropertyChanged(); }
    }

    public string SelectedLogLevelsDisplay
    {
        get
        {
            if (SelectedLogLevels.Count == 0)
                return "All Levels";
            return string.Join(", ", SelectedLogLevels);
        }
    }
    private DateTime timestamp = DateTime.Now;
    private string timeOfDay = DateTime.Now.ToString("HH:mm:ss");
    private int minutesBefore = 5;
    private int minutesAfter = 5;
    private string scanStatus = "Ready";
    private int eventCount = 0;
    private CancellationTokenSource? cts;
    public ObservableCollection<EventLogEntry> Events { get; } = new();
    // Filtered and grouped view
    public ICollectionView EventsView { get; }

    // Log level filter options
    // Start with standard log levels, but add any new ones found in the data
    public ObservableCollection<string> LogLevels { get; } = new() { "Critical", "Error", "Warning", "Information", "Verbose" };

    private void AddLogLevelIfMissing(string level)
    {
        if (!LogLevels.Contains(level))
            LogLevels.Add(level);
    }
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
    // Search filter
    private string searchText = string.Empty;
    public string SearchText
    {
        get => searchText;
        set { searchText = value; OnPropertyChanged(); EventsView.Refresh(); }
    }

    // Grouping toggle
    private bool isGroupingEnabled;
    public bool IsGroupingEnabled
    {
        get => isGroupingEnabled;
        set { isGroupingEnabled = value; OnPropertyChanged(); UpdateGrouping(); }
    }
    private string noEventsMessage = string.Empty;
    public string NoEventsMessage
    {
        get => noEventsMessage;
        set { noEventsMessage = value; OnPropertyChanged(); }
    }

    public DateTime Timestamp
    {
        get => timestamp;
        set { timestamp = value; OnPropertyChanged(); }
    }
    public string TimeOfDay
    {
        get => timeOfDay;
        set { timeOfDay = value; OnPropertyChanged(); }
    }
    public int MinutesBefore
    {
        get => minutesBefore;
        set { minutesBefore = value; OnPropertyChanged(); }
    }
    public int MinutesAfter
    {
        get => minutesAfter;
        set { minutesAfter = value; OnPropertyChanged(); }
    }
    public string ScanStatus
    {
        get => scanStatus;
        set { scanStatus = value; OnPropertyChanged(); }
    }
    public int EventCount
    {
        get => eventCount;
        set { eventCount = value; OnPropertyChanged(); }
    }

    private string lastErrorDetails = string.Empty;
    public string LastErrorDetails
    {
        get => lastErrorDetails;
        set { lastErrorDetails = value; OnPropertyChanged(); }
    }

    private EventLogEntry? selectedEvent;
    public EventLogEntry? SelectedEvent
    {
        get => selectedEvent;
        set { selectedEvent = value; OnPropertyChanged(); }
    }

    public ICommand ScanCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SetNowCommand { get; }
    public ICommand ToggleLogLevelCommand { get; }

    public LiveScanViewModel()
    {
        ScanCommand = new RelayCommand(async _ => await ScanAsync(), _ => cts is null);
        CancelCommand = new RelayCommand(_ => CancelScan(), _ => cts is not null);
        SetNowCommand = new RelayCommand(_ => SetNow());
        ToggleLogLevelCommand = new RelayCommand(param => ToggleLogLevel(param as string));

        // Setup filtered and grouped view
        EventsView = new ListCollectionView(Events);
        EventsView.Filter = FilterEvent;
        UpdateGrouping();
    }

    private void SetNow()
    {
        Timestamp = DateTime.Now;
        TimeOfDay = DateTime.Now.ToString("HH:mm:ss");
    }

    private async Task ScanAsync()
    {
        cts = new CancellationTokenSource();
        ScanStatus = "Scanning...";
        EventCount = 0;
        Events.Clear();
        EventsView.Refresh();
        var service = new EventLogScannerService();
        var scanTimestamp = DateTime.SpecifyKind(Timestamp.Date.Add(TimeSpan.Parse(TimeOfDay)), DateTimeKind.Local).ToUniversalTime();
        var before = TimeSpan.FromMinutes(MinutesBefore);
        var after = TimeSpan.FromMinutes(MinutesAfter);
        var progress = new Progress<(string status, int count)>(tuple =>
        {
            ScanStatus = tuple.status;
            EventCount = tuple.count;
        });
        try
        {
            var results = await service.ScanAllLogsAsync(scanTimestamp, before, after, progress, cts.Token);
            // Ensure ObservableCollection updates are on UI thread
            if (System.Windows.Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
            {
                await dispatcher.InvokeAsync(() => {
                    foreach (var entry in results)
                    {
                        Events.Add(entry);
                        AddLogLevelIfMissing(entry.Level);
                    }
                });
            }
            else
            {
                foreach (var entry in results)
                {
                    Events.Add(entry);
                    AddLogLevelIfMissing(entry.Level);
                }
            }
            ScanStatus = "Scan complete";
            NoEventsMessage = Events.Count == 0 ? "No events found in the selected window." : string.Empty;
        }
        catch (OperationCanceledException)
        {
            ScanStatus = "Scan cancelled";
        }
        catch (Exception ex)
        {
            var details = $"Scan failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            ScanStatus = details;
            NoEventsMessage = details;
            LastErrorDetails = details;
        }
        finally
        {
            cts = null;
        }
    }

    private void CancelScan()
    {
        cts?.Cancel();
    }

    // Filtering logic
    private bool FilterEvent(object obj)
    {
        if (obj is not EventLogEntry entry)
            return false;
        // Log level filter
        if (SelectedLogLevels.Count > 0 && !SelectedLogLevels.Contains(entry.Level))
            return false;
        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchText) && (entry.Message is null || !entry.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
            return false;
        return true;
    }

    // Grouping logic
    private void UpdateGrouping()
    {
        if (EventsView is ListCollectionView lcv)
        {
            lcv.GroupDescriptions.Clear();
            if (IsGroupingEnabled)
                lcv.GroupDescriptions.Add(new PropertyGroupDescription(nameof(EventLogEntry.LogName)));
        }
        EventsView.Refresh();
    }

    private void ToggleLogLevel(string? level)
    {
        if (string.IsNullOrEmpty(level)) return;
        if (SelectedLogLevels.Contains(level))
            SelectedLogLevels.Remove(level);
        else
            SelectedLogLevels.Add(level);
        OnPropertyChanged(nameof(SelectedLogLevelsDisplay));
        EventsView.Refresh();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
