using Janus.App.Services;
using Janus.Core;

using Microsoft.Win32;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Janus.App;

public class RecentSnapshotInfo : INotifyPropertyChanged
{
  public string FilePath { get; set; } = string.Empty;
  public string? Error { get; set; }
  // Replace tuple with explicit properties for binding
  public ScanSession? MetadataSession { get; set; }
  public int? MetadataEventCount { get; set; }

  public string FileName => Path.GetFileName(FilePath);
  public event PropertyChangedEventHandler? PropertyChanged;
  public void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class WelcomeViewModel : INotifyPropertyChanged
{
  private readonly Action<object> setCurrentView;
  public ICommand NewScanCommand { get; }
  public ICommand OpenSnapshotCommand { get; }
  public ICommand OpenRecentSnapshotCommand { get; }
  public ObservableCollection<RecentSnapshotInfo> RecentSnapshots { get; } = [];
  private RecentSnapshotInfo? selectedRecentSnapshot;
  public RecentSnapshotInfo? SelectedRecentSnapshot {
    get => selectedRecentSnapshot;
    set {
      selectedRecentSnapshot = value;
      OnPropertyChanged();
      LoadMetadataForSelectedAsync();
    }
  }
  public event PropertyChangedEventHandler? PropertyChanged;
  private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

  public WelcomeViewModel(Action<object> setCurrentView)
  {
    this.setCurrentView = setCurrentView;
    NewScanCommand = new RelayCommand(_ => StartNewScan());
    OpenSnapshotCommand = new AsyncRelayCommand(OpenSnapshotAsync);
    OpenRecentSnapshotCommand = new AsyncRelayCommand(OpenRecentSnapshotAsync, () => SelectedRecentSnapshot != null);
    _ = LoadRecentSnapshotsAsync();
  }

  private void StartNewScan()
  {
    setCurrentView?.Invoke(new LiveScanViewModel(setCurrentView));
  }

  private async Task OpenSnapshotAsync()
  {
    var openFileDialog = new OpenFileDialog {
      Filter = "Janus Snapshot (*.janus)|*.janus",
      DefaultExt = "janus"
    };
    if (openFileDialog.ShowDialog() == true) {
      await OpenSnapshotFileAsync(openFileDialog.FileName);
    }
  }

  private async Task OpenRecentSnapshotAsync()
  {
    if (SelectedRecentSnapshot == null) return;
    await OpenSnapshotFileAsync(SelectedRecentSnapshot.FilePath);
  }

  private async Task OpenSnapshotFileAsync(string filePath)
  {
    var service = new SnapshotService();
    try {
      var session = await SnapshotService.LoadSnapshotAsync(filePath);
      if (session is not null) {
        AddRecentSnapshot(filePath);
        var resultsVm = new ResultsViewModel(setCurrentView, ResultsViewModel.PreviousView.Welcome);
        resultsVm.LoadEvents(session.Entries);
        resultsVm.SetMetadata(session);
        setCurrentView?.Invoke(new ResultsView { DataContext = resultsVm });
      } else {
        MessageBox.Show("No snapshot data found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    } catch (Exception ex) {
      MessageBox.Show($"Failed to load snapshot: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  private async Task LoadRecentSnapshotsAsync()
  {
    var settings = await UserUiSettingsService.LoadAsync();
    RecentSnapshots.Clear();
    foreach (var path in settings.RecentSnapshots.Distinct().Take(10)) {
      if (!File.Exists(path)) continue;
      RecentSnapshots.Add(new RecentSnapshotInfo { FilePath = path });
    }
  }

  private async void AddRecentSnapshot(string filePath)
  {
    await UserUiSettingsService.AddRecentSnapshotAsync(filePath);
    await LoadRecentSnapshotsAsync();
  }

  private async void LoadMetadataForSelectedAsync()
  {
    if (SelectedRecentSnapshot == null) return;
    if (!File.Exists(SelectedRecentSnapshot.FilePath)) {
      SelectedRecentSnapshot.Error = "File not found.";
      SelectedRecentSnapshot.MetadataSession = null;
      SelectedRecentSnapshot.MetadataEventCount = null;
      SelectedRecentSnapshot.OnPropertyChanged(nameof(RecentSnapshotInfo.Error));
      SelectedRecentSnapshot.OnPropertyChanged(nameof(RecentSnapshotInfo.MetadataSession));
      SelectedRecentSnapshot.OnPropertyChanged(nameof(RecentSnapshotInfo.MetadataEventCount));
      return;
    }
    var service = new SnapshotService();
    try {
      var sessioninfo = await SnapshotService.LoadSnapshotMetadataAsync(SelectedRecentSnapshot.FilePath);
      SelectedRecentSnapshot.MetadataSession = sessioninfo.Item1;
      SelectedRecentSnapshot.MetadataEventCount = sessioninfo.Item2;
      SelectedRecentSnapshot.Error = null;
    } catch (Exception ex) {
      SelectedRecentSnapshot.MetadataSession = null;
      SelectedRecentSnapshot.MetadataEventCount = null;
      SelectedRecentSnapshot.Error = ex.Message;
    }
    SelectedRecentSnapshot.OnPropertyChanged(nameof(RecentSnapshotInfo.MetadataSession));
    SelectedRecentSnapshot.OnPropertyChanged(nameof(RecentSnapshotInfo.MetadataEventCount));
    SelectedRecentSnapshot.OnPropertyChanged(nameof(RecentSnapshotInfo.Error));
  }
}
