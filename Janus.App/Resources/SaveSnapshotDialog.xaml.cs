using System.Windows;

namespace Janus.App;

public partial class SaveSnapshotDialog : Window {
  public SaveSnapshotDialog() {
    InitializeComponent();
    DataContext = new SaveSnapshotDialogViewModel(this);
  }
}
