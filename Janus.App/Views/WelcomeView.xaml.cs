using System.Windows.Controls;
using System.Windows.Input;

namespace Janus.App;

public partial class WelcomeView : UserControl {
  public WelcomeView() {
    InitializeComponent();
    this.Loaded += WelcomeView_Loaded;
    RecentSnapshotsListBox.KeyDown += RecentSnapshotsListBox_KeyDown;
  }

  private void RecentSnapshotsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
    if (DataContext is WelcomeViewModel vm && sender is ListBox lb && lb.SelectedItem is RecentSnapshotInfo info) {
      if (vm.OpenRecentSnapshotCommand.CanExecute(info)) {
        vm.OpenRecentSnapshotCommand.Execute(info);
      }
    }
  }

  private void WelcomeView_Loaded(object sender, System.Windows.RoutedEventArgs e) {
    if (DataContext is WelcomeViewModel && RecentSnapshotsListBox.Items.Count > 0 && RecentSnapshotsListBox.SelectedIndex == -1) {
      RecentSnapshotsListBox.SelectedIndex = 0;
    }
    NewScanButton.Focus();
  }

  private void RecentSnapshotsListBox_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Enter && DataContext is WelcomeViewModel vm && RecentSnapshotsListBox.SelectedItem is RecentSnapshotInfo info) {
      if (vm.OpenRecentSnapshotCommand.CanExecute(info)) {
        vm.OpenRecentSnapshotCommand.Execute(info);
      }

      e.Handled = true;
    }
  }
}
