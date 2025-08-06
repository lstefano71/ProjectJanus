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
    this.Loaded += ResultsView_Loaded;
  }

  private void ResultsView_Loaded(object sender, RoutedEventArgs e)
  {
    //    _ = FindName("ResultsSplitter") as GridSplitter;
    // _ = FindName("DetailsSplitter") as GridSplitter; 
    if (ResultsDataGrid != null) {
      ResultsDataGrid.KeyDown += ResultsDataGrid_KeyDown;
      ResultsDataGrid.Focus();
      if (ResultsDataGrid.Items.Count > 0 && ResultsDataGrid.SelectedIndex == -1)
        ResultsDataGrid.SelectedIndex = 0;
    }
    // Remove SearchBox logic: now handled by TextBoxWithPlaceholder
  }

  private void ResultsDataGrid_KeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key == Key.Space) {
      if (sender is DataGrid dg && dg.SelectedItem is EventLogEntryDisplay entry) {
        if (DataContext is ResultsViewModel vm && vm.ToggleCheckedCommand.CanExecute(entry)) {
          vm.ToggleCheckedCommand.Execute(entry);
          e.Handled = true;
        }
      }
    } else if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
      if (DataContext is ResultsViewModel vm && vm.CopyMessageCommand.CanExecute(null)) {
        vm.CopyMessageCommand.Execute(null);
        e.Handled = true;
      }
    } else if (e.Key == Key.Enter) {
      if (DataContext is ResultsViewModel vm && vm.CopyMessageCommand.CanExecute(null)) {
        vm.CopyMessageCommand.Execute(null);
        e.Handled = true;
      }
    }
  }
}
