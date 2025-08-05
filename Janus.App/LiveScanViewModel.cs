using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Janus.Core;

namespace Janus.App;

public class LiveScanViewModel : INotifyPropertyChanged
{
    private DateTime timestamp = DateTime.Now;
    private string timeOfDay = DateTime.Now.ToString("HH:mm:ss");
    private int minutesBefore = 5;
    private int minutesAfter = 5;
    private string scanStatus = "Ready";
    private int eventCount = 0;
    private CancellationTokenSource? cts;

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
        var service = new EventLogScannerService();
        var scanTimestamp = Timestamp.Date.Add(TimeSpan.Parse(TimeOfDay));
        var window = TimeSpan.FromMinutes(MinutesBefore + MinutesAfter);
        var progress = new Progress<(string status, int count)>(tuple =>
        {
            ScanStatus = tuple.status;
            EventCount = tuple.count;
        });
        try
        {
            await service.ScanAllLogsAsync(scanTimestamp, window, progress, cts.Token);
            ScanStatus = "Scan complete";
        }
        catch (OperationCanceledException)
        {
            ScanStatus = "Scan cancelled";
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
