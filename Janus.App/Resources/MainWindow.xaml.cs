using Janus.App.Services;

using System.Windows;

namespace Janus.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
  private readonly UserUiSettingsService uiSettingsService = UserUiSettingsService.Instance;
  private readonly Debouncer windowSizeDebouncer = new(TimeSpan.FromMilliseconds(500));

  public MainWindow() {
    // Load settings synchronously before window is shown
    Initialized += async (s, e) => {
      try {
        UserUiSettingsService.UiSettings settings = await UserUiSettingsService.LoadAsync();
        Width = settings.MainWindowWidth;
        Height = settings.MainWindowHeight;
      } catch (Exception ex) {
        // Handle exceptions if needed, e.g., log them
        MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    };
    InitializeComponent();
    DataContext = new MainWindowViewModel();
    SizeChanged += MainWindow_SizeChanged;

  }

  private void MainWindow_SizeChanged(object? sender, SizeChangedEventArgs e) => windowSizeDebouncer.Debounce(SaveWindowSizeAsync);

  private async Task SaveWindowSizeAsync() {
    UserUiSettingsService.UiSettings settings = await UserUiSettingsService.LoadAsync();
    settings.MainWindowWidth = Width;
    settings.MainWindowHeight = Height;
    await UserUiSettingsService.SaveAsync(settings);
  }

  private void OnAboutClick(object sender, RoutedEventArgs e) {
    var about = new AboutDialog();
    about.ShowDialog();
  }
}