using Janus.App.Services;
using Janus.Core;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Janus.App;

public class LiveScanViewModel : INotifyPropertyChanged {
  private readonly Action<object>? setCurrentView;
  private readonly UserUiSettingsService uiSettingsService = UserUiSettingsService.Instance;
  private DateTime timestamp = DateTime.Now;
  private string timeOfDay = DateTime.Now.ToString("HH:mm:ss");
  private int minutesBefore = 5;
  private int minutesAfter = 5;
  private string scanStatus = "Ready";
  private CancellationTokenSource? cts;

  private bool isScanInProgress;

  // Thread-safe counters
  private int totalSources;
  private int sourcesCompletedSuccess;
  private int sourcesCompletedError;
  private int sourcesInProgress;
  private int totalEvents;

  private System.Collections.ObjectModel.ObservableCollection<SourceScanProgress> scannedSources = [];
  public System.Collections.ObjectModel.ObservableCollection<SourceScanProgress> ScannedSources {
    get => scannedSources;
    set {
      if (scannedSources != value) {
        scannedSources = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(TotalSources));
        OnPropertyChanged(nameof(SourcesCompletedSuccess));
        OnPropertyChanged(nameof(SourcesCompletedError));
        OnPropertyChanged(nameof(SourcesInProgress));
        OnPropertyChanged(nameof(TotalEvents));
      }
    }
  }

  public int TotalSources {
    get => totalSources;
    private set { totalSources = value; OnPropertyChanged(); }
  }
  public int SourcesCompletedSuccess {
    get => sourcesCompletedSuccess;
    private set { sourcesCompletedSuccess = value; OnPropertyChanged(); }
  }
  public int SourcesCompletedError {
    get => sourcesCompletedError;
    private set { sourcesCompletedError = value; OnPropertyChanged(); }
  }
  public int SourcesInProgress {
    get => sourcesInProgress;
    private set { sourcesInProgress = value; OnPropertyChanged(); }
  }
  public int TotalEvents {
    get => totalEvents;
    private set { totalEvents = value; OnPropertyChanged(); }
  }

  private void ResetStats() {
    TotalSources = 0;
    SourcesCompletedSuccess = 0;
    SourcesCompletedError = 0;
    SourcesInProgress = 0;
    TotalEvents = 0;
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

  public LiveScanViewModel(Action<object>? setCurrentView = null) {
    this.setCurrentView = setCurrentView;
    ScanCommand = new AsyncRelayCommand(ScanAsync, () => cts is null);
    CancelCommand = new RelayCommand(_ => CancelScan(), _ => cts is not null);
    SetNowCommand = new RelayCommand(_ => SetNow());
    BackCommand = new RelayCommand(_ => GoBack(), _ => setCurrentView != null && !IsScanInProgress);
    _ = LoadUiSettingsAsync();
  }

  private void GoBack() => setCurrentView?.Invoke(new WelcomeViewModel(setCurrentView!));

  private async Task LoadUiSettingsAsync() {
    UserUiSettingsService.UiSettings settings = await UserUiSettingsService.LoadAsync();
    MinutesBefore = settings.MinutesBefore;
    MinutesAfter = settings.MinutesAfter;
  }

  private async Task SaveUiSettingsAsync() {
    UserUiSettingsService.UiSettings settings = await UserUiSettingsService.LoadAsync();
    settings.MinutesBefore = MinutesBefore;
    settings.MinutesAfter = MinutesAfter;
    await UserUiSettingsService.SaveAsync(settings);
  }

  private void SetNow() {
    Timestamp = DateTime.Now;
    TimeOfDay = DateTime.Now.ToString("HH:mm:ss");
  }

  private async Task ScanAsync() {
    cts = new CancellationTokenSource();
    IsScanInProgress = true;
    ScanStatus = "Scanning...";
    ScannedSources.Clear();

    DateTime scanTimestamp = DateTime.SpecifyKind(Timestamp.Date.Add(TimeSpan.Parse(TimeOfDay)), DateTimeKind.Local).ToUniversalTime();
    var before = TimeSpan.FromMinutes(MinutesBefore);
    var after = TimeSpan.FromMinutes(MinutesAfter);

    System.Windows.Threading.Dispatcher? dispatcher = System.Windows.Application.Current?.Dispatcher;

    ResetStats();
    var progress = new Progress<SourceScanProgress>(progressUpdate => {
      void update() {
        SourceScanProgress? existing = ScannedSources.FirstOrDefault(s => s.SourceName == progressUpdate.SourceName);
        if (existing == null) {
          ScannedSources.Insert(0, progressUpdate);
          TotalSources++;
          if (progressUpdate.IsActive) {
            SourcesInProgress++;
          } else {
            if (progressUpdate.Status == Core.ScanStatus.Success) {
              SourcesCompletedSuccess++;
            } else if (progressUpdate.Status == Core.ScanStatus.Failed) {
              SourcesCompletedError++;
            }
          }
        } else {
          // Track event count delta
          int prevEvents = existing.EventsRetrieved;
          existing.EventsRetrieved = progressUpdate.EventsRetrieved;
          TotalEvents += (progressUpdate.EventsRetrieved - prevEvents);

          bool wasActive = existing.IsActive;
          ScanStatus prevStatus = existing.Status;

          existing.Status = progressUpdate.Status;
          existing.IsTotalKnown = progressUpdate.IsTotalKnown;
          existing.TotalEvents = progressUpdate.TotalEvents;
          existing.IsActive = progressUpdate.IsActive;
          existing.ExceptionMessage = progressUpdate.ExceptionMessage;
          // Notify property changed for all properties
          existing.OnPropertyChanged(nameof(existing.EventsRetrieved));
          existing.OnPropertyChanged(nameof(existing.Status));
          existing.OnPropertyChanged(nameof(existing.IsTotalKnown));
          existing.OnPropertyChanged(nameof(existing.TotalEvents));
          existing.OnPropertyChanged(nameof(existing.IsActive));
          existing.OnPropertyChanged(nameof(existing.ExceptionMessage));

          // Transition from active to not active
          if (wasActive && !progressUpdate.IsActive) {
            SourcesInProgress--;
            if (progressUpdate.Status == Core.ScanStatus.Success) {
              SourcesCompletedSuccess++;
            } else if (progressUpdate.Status == Core.ScanStatus.Failed) {
              SourcesCompletedError++;
            }
          }
        }
        // Move most recently active to top
        if (progressUpdate.IsActive && existing != null) {
          ScannedSources.Move(ScannedSources.IndexOf(existing), 0);
        }
        // Update ScanStatus with statistics
        ScanStatus = $"Sources: {TotalSources}/Success: {SourcesCompletedSuccess}/Errors: {SourcesCompletedError}/In Progress: {SourcesInProgress} | Events: {TotalEvents}";
      }
      if (dispatcher != null && !dispatcher.CheckAccess()) {
        dispatcher.Invoke(update);
      } else {
        update();
      }
    });

    try {
      ScanResult scanResult = await EventLogScannerService.ScanAllLogsAsync(scanTimestamp, before, after, progress, cts.Token);
      ScanStatus = "Scan complete";
      // NAVIGATE TO RESULTS VIEW
      if (setCurrentView != null) {
        var resultsVm = new ResultsViewModel(setCurrentView, ResultsViewModel.PreviousView.LiveScan);
        resultsVm.LoadEvents(scanResult.Entries);
        resultsVm.SetMetadata(new ScanSession {
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
    } catch (OperationCanceledException) {
      ScanStatus = "Scan cancelled";
    } catch (Exception ex) {
      string details = $"Scan failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
      ScanStatus = details;
    } finally {
      cts = null;
      IsScanInProgress = false;
    }
  }

  private void CancelScan() => cts?.Cancel();

  public event PropertyChangedEventHandler? PropertyChanged;
  private void OnPropertyChanged([CallerMemberName] string? name = null)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
