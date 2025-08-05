using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Janus.App.Services;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace Janus.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly UserUiSettingsService uiSettingsService = UserUiSettingsService.Instance;
    private readonly DispatcherTimer debounceTimer;
    private bool pendingSave;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        debounceTimer.Tick += async (_, __) =>
        {
            if (pendingSave)
            {
                await SaveWindowSizeAsync();
                pendingSave = false;
            }
        };
        Loaded += async (s, e) => await MainWindow_LoadedAsync();
        SizeChanged += MainWindow_SizeChanged;
    }

    private async Task MainWindow_LoadedAsync()
    {
        var settings = await uiSettingsService.LoadAsync();
        Width = settings.MainWindowWidth;
        Height = settings.MainWindowHeight;
    }

    private void MainWindow_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        DebounceSave();
    }

    private void DebounceSave()
    {
        pendingSave = true;
        debounceTimer.Stop();
        debounceTimer.Start();
    }

    private async Task SaveWindowSizeAsync()
    {
        var settings = await uiSettingsService.LoadAsync();
        settings.MainWindowWidth = Width;
        settings.MainWindowHeight = Height;
        await uiSettingsService.SaveAsync(settings);
    }

    private void OnAboutClick(object sender, RoutedEventArgs e)
    {
        var about = new AboutDialog();
        about.ShowDialog();
    }
}