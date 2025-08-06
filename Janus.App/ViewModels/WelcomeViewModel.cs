using Microsoft.Win32;

using System.Windows;
using System.Windows.Input;

namespace Janus.App;

public class WelcomeViewModel
{
  private readonly Action<object> setCurrentView;
  public ICommand NewScanCommand { get; }
  public ICommand OpenSnapshotCommand { get; }

  public WelcomeViewModel(Action<object> setCurrentView)
  {
    this.setCurrentView = setCurrentView;
    NewScanCommand = new RelayCommand(_ => StartNewScan());
    OpenSnapshotCommand = new RelayCommand(_ => OpenSnapshot());
  }

  private void StartNewScan()
  {
    setCurrentView?.Invoke(new LiveScanViewModel(setCurrentView));
  }

  private async void OpenSnapshot()
  {
    var openFileDialog = new OpenFileDialog {
      Filter = "Janus Snapshot (*.janus)|*.janus",
      DefaultExt = "janus"
    };
    if (openFileDialog.ShowDialog() == true) {
      var service = new SnapshotService();
      try {
        var session = await service.LoadSnapshotAsync(openFileDialog.FileName);
        if (session is not null) {
          var resultsVm = new ResultsViewModel();
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
  }
}
