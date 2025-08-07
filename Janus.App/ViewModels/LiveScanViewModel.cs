using Janus.App.Services;
using Janus.Core;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Janus.App;

public class LiveScanViewModel : INotifyPropertyChanged
{
  private readonly Action<object>? setCurrentView;
  private readonly UserUiSettingsService uiSettingsService = UserUiSettingsService.Instance;
  private DateTime timestamp = DateTime.Now;
  private string timeOfDay = DateTime.Now.ToString("HH:mm:ss");
  private int minutesBefore = 5;
  private int minutesAfter = 5;
  private string scanStatus = "Ready";
  private CancellationTokenSource? cts;

  private bool isScanInProgress;

  private System.Collections.ObjectModel.ObservableCollection<SourceScanProgress> scannedSources = new();
  public System.Collections.ObjectModel.ObservableCollection<SourceScanProgress> ScannedSources
  {
    get => scannedSources;
    set
    {
      if (scannedSources != value)
      {
        scannedSources = value;
        OnPropertyChanged();
      }
    }
  }
  public DateTime Timestamp {
    get => timestamp;
    set { timestamp = value; OnPropertyChanged(); }
  }
  public string TimeOfDay {
    get => timeOfDay;
    set { timeOfDay = value; OnPropertyChanged(); }
  }
  public int MinutesBefore {
    get => minutesBefore;
    set {
      if (minutesBefore != value) {
        minutesBefore = value;
        OnPropertyChanged();
        _ = SaveUiSettingsAsync();
      }
    }
  }
  public int MinutesAfter {
    get => minutesAfter;
    set {
      if (minutesAfter != value) {
        minutesAfter = value;
        OnPropertyChanged();
        _ = SaveUiSettingsAsync();
      }
    }
  }
  public string ScanStatus {
    get => scanStatus;
    set { scanStatus = value; OnPropertyChanged(); }
  }
  public bool IsScanInProgress {
    get => isScanInProgress;
    set {
      if (isScanInProgress != value) {
        isScanInProgress = value;
        OnPropertyChanged();
        RelayCommand.RaiseCanExecuteChanged();
      }
    }
  }

  public ICommand ScanCommand { get; }
  public ICommand CancelCommand { get; }
  public ICommand SetNowCommand { get; }
  public ICommand BackCommand { get; }

  public LiveScanViewModel(Action<object>? setCurrentView = null)
  {
    this.setCurrentView = setCurrentView;
    ScanCommand = new AsyncRelayCommand(ScanAsync, () => cts is null);
    CancelCommand = new RelayCommand(_ => CancelScan(), _ => cts is not null);
    SetNowCommand = new RelayCommand(_ => SetNow());
    BackCommand = new RelayCommand(_ => GoBack(), _ => setCurrentView != null && !IsScanInProgress);
    _ = LoadUiSettingsAsync();
  }

  private void GoBack()
  {
    setCurrentView?.Invoke(new WelcomeViewModel(setCurrentView!));
  }

  private async Task LoadUiSettingsAsync()
  {
    var settings = await UserUiSettingsService.LoadAsync();
    MinutesBefore = settings.MinutesBefore;
    MinutesAfter = settings.MinutesAfter;
  }

  private async Task SaveUiSettingsAsync()
  {
    var settings = await UserUiSettingsService.LoadAsync();
    settings.MinutesBefore = MinutesBefore;
    settings.MinutesAfter = MinutesAfter;
    await UserUiSettingsService.SaveAsync(settings);
  }

  private void SetNow()
  {
    Timestamp = DateTime.Now;
    TimeOfDay = DateTime.Now.ToString("HH:mm:ss");
  }

  private async Task ScanAsync()
  {
    cts = new CancellationTokenSource();
    IsScanInProgress = true;
    ScanStatus = "Scanning...";
    ScannedSources.Clear();

    var scanTimestamp = DateTime.SpecifyKind(Timestamp.Date.Add(TimeSpan.Parse(TimeOfDay)), DateTimeKind.Local).ToUniversalTime();
    var before = TimeSpan.FromMinutes(MinutesBefore);
    var after = TimeSpan.FromMinutes(MinutesAfter);

    var dispatcher = System.Windows.Application.Current?.Dispatcher;

    var progress = new Progress<SourceScanProgress>(progressUpdate =>
    {
      void update()
      {
        var existing = ScannedSources.FirstOrDefault(s => s.SourceName == progressUpdate.SourceName);
        if (existing == null)
        {
          ScannedSources.Insert(0, progressUpdate);
        }
        else
        {
          existing.EventsRetrieved = progressUpdate.EventsRetrieved;
          existing.Status = progressUpdate.Status;
          existing.IsTotalKnown = progressUpdate.IsTotalKnown;
          existing.TotalEvents = progressUpdate.TotalEvents;
          existing.IsActive = progressUpdate.IsActive;
          // Notify property changed for all properties
          existing.OnPropertyChanged(nameof(existing.EventsRetrieved));
          existing.OnPropertyChanged(nameof(existing.Status));
          existing.OnPropertyChanged(nameof(existing.IsTotalKnown));
          existing.OnPropertyChanged(nameof(existing.TotalEvents));
          existing.OnPropertyChanged(nameof(existing.IsActive));
        }
        // Move most recently active to top
        if (progressUpdate.IsActive && existing != null)
        {
          ScannedSources.Move(ScannedSources.IndexOf(existing), 0);
        }
      }
      if (dispatcher != null && !dispatcher.CheckAccess())
        dispatcher.Invoke(update);
      else
        update();
    });

    try
    {
      var scanResult = await EventLogScannerService.ScanAllLogsAsync(scanTimestamp, before, after, progress, cts.Token);
      ScanStatus = "Scan complete";
      // NAVIGATE TO RESULTS VIEW
      if (setCurrentView != null)
      {
        var resultsVm = new ResultsViewModel(setCurrentView, ResultsViewModel.PreviousView.LiveScan);
        resultsVm.LoadEvents(scanResult.Entries);
        resultsVm.SetMetadata(new ScanSession
        {
          Id = Guid.NewGuid(),
          Timestamp = scanTimestamp,
          MinutesBefore = MinutesBefore,
          MinutesAfter = MinutesAfter,
          SnapshotCreated = DateTime.UtcNow,
          MachineName = Environment.MachineName,
          UserNotes = string.Empty,
          Entries = [.. scanResult.Entries],
          ScannedSources = [.. scanResult.ScannedSources]
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
      IsScanInProgress = false;
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
