using Janus.App.Services;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Janus.App;

public partial class ResultsView : UserControl
{
  private readonly UserUiSettingsService uiSettings = UserUiSettingsService.Instance;
  private readonly Debouncer debouncer = new(TimeSpan.FromMilliseconds(500));
  public ResultsView()
  {
    InitializeComponent();
    // Attach key handler for DataGrid after InitializeComponent
    this.Loaded += (s, e) => {
      var dataGrid = this.FindName("ResultsDataGrid") as DataGrid;
      if (dataGrid != null) {
        dataGrid.PreviewKeyDown += DataGrid_PreviewKeyDown;
      }
    };
  }

  private void UserControl_Loaded(object sender, RoutedEventArgs e)
  {
    _ = FindName("ResultsSplitter") as GridSplitter;
    _ = FindName("DetailsSplitter") as GridSplitter;
  }

  private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key == Key.Space) {
      if (sender is DataGrid dg && dg.SelectedItem is EventLogEntryDisplay entry) {
        if (DataContext is ResultsViewModel vm && vm.ToggleCheckedCommand.CanExecute(entry)) {
          vm.ToggleCheckedCommand.Execute(entry);
          e.Handled = true;
        }
      }
    }
  }
}
