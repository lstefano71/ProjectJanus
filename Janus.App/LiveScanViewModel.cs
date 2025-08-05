using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Janus.Core;

namespace Janus.App;

using System.Collections.ObjectModel;

public class LiveScanViewModel : INotifyPropertyChanged
{
    private DateTime timestamp = DateTime.Now;
    private string timeOfDay = DateTime.Now.ToString("HH:mm:ss");
    private int minutesBefore = 5;
    private int minutesAfter = 5;
    private string scanStatus = "Ready";
    private int eventCount = 0;
    private CancellationTokenSource? cts;
    public ObservableCollection<EventLogEntry> Events { get; } = new();
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

    public LiveScanViewModel()
    {
        ScanCommand = new RelayCommand(async _ => await ScanAsync(), _ => cts is null);
        CancelCommand = new RelayCommand(_ => CancelScan(), _ => cts is not null);
        SetNowCommand = new RelayCommand(_ => SetNow());
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
                        Events.Add(entry);
                });
            }
            else
            {
                foreach (var entry in results)
                    Events.Add(entry);
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

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
