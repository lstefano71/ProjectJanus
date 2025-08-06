using System.Windows.Controls;
using System.Windows.Input;

namespace Janus.App;

public partial class WelcomeView : UserControl
{
  public WelcomeView()
  {
    InitializeComponent();
    this.Loaded += WelcomeView_Loaded;
  }

  private void RecentSnapshotsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
  {
    if (DataContext is WelcomeViewModel vm && sender is ListBox lb && lb.SelectedItem is RecentSnapshotInfo info)
    {
      if (vm.OpenRecentSnapshotCommand.CanExecute(info))
        vm.OpenRecentSnapshotCommand.Execute(info);
    }
  }

  private void WelcomeView_Loaded(object sender, System.Windows.RoutedEventArgs e)
  {
    if (DataContext is WelcomeViewModel vm && RecentSnapshotsListBox.Items.Count > 0 && RecentSnapshotsListBox.SelectedIndex == -1)
    {
      RecentSnapshotsListBox.SelectedIndex = 0;
    }
  }
}
