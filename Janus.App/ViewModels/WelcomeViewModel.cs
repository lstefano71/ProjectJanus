using Janus.App.Services;
using Janus.Core;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Janus.App;

public class RecentSnapshotInfo : INotifyPropertyChanged
{
    public string FilePath { get; set; } = string.Empty;
    public string? Error { get; set; }
    public ScanSession? Metadata { get; set; }
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
    public ObservableCollection<RecentSnapshotInfo> RecentSnapshots { get; } = new();
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
            var session = await service.LoadSnapshotAsync(filePath);
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
        var settings = await UserUiSettingsService.Instance.LoadAsync();
        RecentSnapshots.Clear();
        foreach (var path in settings.RecentSnapshots.Distinct().Take(10))
        {
            if (!File.Exists(path)) continue;
            RecentSnapshots.Add(new RecentSnapshotInfo { FilePath = path });
        }
    }

    private async void AddRecentSnapshot(string filePath)
    {
        await UserUiSettingsService.Instance.AddRecentSnapshotAsync(filePath);
        await LoadRecentSnapshotsAsync();
    }

    private async void LoadMetadataForSelectedAsync()
    {
        if (SelectedRecentSnapshot == null) return;
        if (!File.Exists(SelectedRecentSnapshot.FilePath)) {
            SelectedRecentSnapshot.Error = "File not found.";
            SelectedRecentSnapshot.Metadata = null;
            SelectedRecentSnapshot.OnPropertyChanged(nameof(RecentSnapshotInfo.Error));
            SelectedRecentSnapshot.OnPropertyChanged(nameof(RecentSnapshotInfo.Metadata));
            return;
        }
        var service = new SnapshotService();
        try {
            var session = await service.LoadSnapshotAsync(SelectedRecentSnapshot.FilePath);
            SelectedRecentSnapshot.Metadata = session;
            SelectedRecentSnapshot.Error = null;
        } catch (Exception ex) {
            SelectedRecentSnapshot.Metadata = null;
            SelectedRecentSnapshot.Error = ex.Message;
        }
        SelectedRecentSnapshot.OnPropertyChanged(nameof(RecentSnapshotInfo.Metadata));
        SelectedRecentSnapshot.OnPropertyChanged(nameof(RecentSnapshotInfo.Error));
    }
}
