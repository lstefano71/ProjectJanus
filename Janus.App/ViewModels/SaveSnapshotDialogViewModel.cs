using System.Windows;
using System.Windows.Input;

namespace Janus.App;

public class SaveSnapshotDialogViewModel {
  public string UserNotes { get; set; } = string.Empty;
  public ICommand SaveCommand { get; }
  public ICommand CancelCommand { get; }
  private readonly Window dialog;

  public SaveSnapshotDialogViewModel(Window dialog) {
    this.dialog = dialog;
    SaveCommand = new RelayCommand(_ => Save());
    CancelCommand = new RelayCommand(_ => Cancel());
  }

  private void Save() {
    dialog.DialogResult = true;
    dialog.Close();
  }

  private void Cancel() {
    dialog.DialogResult = false;
    dialog.Close();
  }
}
