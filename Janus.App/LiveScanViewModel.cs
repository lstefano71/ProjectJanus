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
    private readonly Action<object>? setCurrentView;
    private DateTime timestamp = DateTime.Now;
    private string timeOfDay = DateTime.Now.ToString("HH:mm:ss");
    private int minutesBefore = 5;
    private int minutesAfter = 5;
    private string scanStatus = "Ready";
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

    public ICommand ScanCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SetNowCommand { get; }

    public LiveScanViewModel(Action<object>? setCurrentView = null)
    {
        this.setCurrentView = setCurrentView;
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
        var service = new EventLogScannerService();
        var scanTimestamp = DateTime.SpecifyKind(Timestamp.Date.Add(TimeSpan.Parse(TimeOfDay)), DateTimeKind.Local).ToUniversalTime();
        var before = TimeSpan.FromMinutes(MinutesBefore);
        var after = TimeSpan.FromMinutes(MinutesAfter);
        var progress = new Progress<(string status, int count)>(tuple =>
        {
            ScanStatus = tuple.status;
        });
        try
        {
            var results = await service.ScanAllLogsAsync(scanTimestamp, before, after, progress, cts.Token);
            ScanStatus = "Scan complete";
            // NAVIGATE TO RESULTS VIEW
            if (setCurrentView != null)
            {
                var resultsVm = new ResultsViewModel();
                resultsVm.LoadEvents(results);
                resultsVm.SetMetadata(new ScanSession {
                    Id = Guid.NewGuid(),
                    Timestamp = scanTimestamp,
                    MinutesBefore = MinutesBefore,
                    MinutesAfter = MinutesAfter,
                    SnapshotCreated = DateTime.UtcNow,
                    MachineName = Environment.MachineName,
                    UserNotes = string.Empty,
                    Entries = results.ToList()
                });
                setCurrentView(new ResultsView { DataContext = resultsVm });
            }
        }
        catch (OperationCanceledException)
        {
            ScanStatus = "Scan cancelled";
        }
        catch (Exception ex)
        {
            var details = $"Scan failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            ScanStatus = details;
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
