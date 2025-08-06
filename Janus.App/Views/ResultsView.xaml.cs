using Janus.App.Services;

using System.Windows;
using System.Windows.Controls;

namespace Janus.App;

public partial class ResultsView : UserControl
{
  private readonly UserUiSettingsService uiSettings = UserUiSettingsService.Instance;
  private readonly Debouncer debouncer = new(TimeSpan.FromMilliseconds(500));
  public ResultsView()
  {
    InitializeComponent();
  }

  private void UserControl_Loaded(object sender, RoutedEventArgs e)
  {
    _ = FindName("ResultsSplitter") as GridSplitter;
    _ = FindName("DetailsSplitter") as GridSplitter;

    //hSplitter.= uiSettings.UiSettings.ResultsSplitterPosition;
    //vSplitter.Position = uiSettings.UiSettings.DetailsSplitterPosition;
  }
}
