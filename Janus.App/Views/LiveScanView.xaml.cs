using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Janus.App;

public partial class LiveScanView : UserControl
{
  public LiveScanView()
  {
    InitializeComponent();
    Loaded += LiveScanView_Loaded;
    TimeOfDayBox.GotKeyboardFocus += SelectAllTextBox;
    MinutesBeforeBox.GotKeyboardFocus += SelectAllTextBox;
    MinutesAfterBox.GotKeyboardFocus += SelectAllTextBox;
    MinutesBeforeBox.PreviewTextInput += NumericOnly_PreviewTextInput;
    MinutesAfterBox.PreviewTextInput += NumericOnly_PreviewTextInput;
    DataObject.AddPastingHandler(MinutesBeforeBox, NumericOnly_Pasting);
    DataObject.AddPastingHandler(MinutesAfterBox, NumericOnly_Pasting);
  }

  private void LiveScanView_Loaded(object sender, RoutedEventArgs e)
  {
    // Set initial focus to the date picker
    TimestampPicker.Focus();
    TimestampPicker.Dispatcher.BeginInvoke(() => TimestampPicker.IsDropDownOpen = true);
  }

  private void SelectAllTextBox(object sender, KeyboardFocusChangedEventArgs e)
  {
    if (sender is TextBox tb)
      tb.SelectAll();
  }

  private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
  {
    e.Handled = !IsTextNumeric(e.Text);
  }

  private void NumericOnly_Pasting(object sender, DataObjectPastingEventArgs e)
  {
    if (e.DataObject.GetDataPresent(typeof(string))) {
      var text = (string)e.DataObject.GetData(typeof(string));
      if (!IsTextNumeric(text))
        e.CancelCommand();
    } else {
      e.CancelCommand();
    }
  }

  private static bool IsTextNumeric(string text)
  {
    foreach (char c in text)
      if (!char.IsDigit(c))
        return false;
    return true;
  }
}
