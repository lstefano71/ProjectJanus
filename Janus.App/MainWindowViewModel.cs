using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Janus.App;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private object? currentView;
    public object? CurrentView
    {
        get => currentView;
        set { currentView = value; OnPropertyChanged(); }
    }

    public MainWindowViewModel()
    {
        CurrentView = new WelcomeViewModel(SetCurrentView);
    }

    public void SetCurrentView(object view)
    {
        CurrentView = view;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
