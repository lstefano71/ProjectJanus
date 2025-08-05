using System.Windows.Input;

namespace Janus.App;

public class SaveSnapshotDialogViewModel
{
    public string UserNotes { get; set; } = string.Empty;
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public SaveSnapshotDialogViewModel()
    {
        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    private void Save()
    {
        // Logic to save snapshot with notes
    }

    private void Cancel()
    {
        // Logic to close dialog without saving
    }
}
