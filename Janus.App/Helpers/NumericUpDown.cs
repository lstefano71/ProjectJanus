using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Janus.App;

public partial class NumericUpDown : UserControl, INotifyPropertyChanged {
  public static readonly DependencyProperty ValueProperty =
    DependencyProperty.Register(
      nameof(Value),
      typeof(int),
      typeof(NumericUpDown),
      new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

  public int Value {
    get => (int)GetValue(ValueProperty);
    set => SetValue(ValueProperty, value);
  }

  public ICommand IncrementCommand { get; }
  public ICommand DecrementCommand { get; }

  public NumericUpDown() {
    InitializeComponent();
    DataContext = this;
    IncrementCommand = new RelayCommand(_ => Value++);
    DecrementCommand = new RelayCommand(_ => Value--);
  }

  private void ValueBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
    if (sender is TextBox tb) {
      tb.SelectAll();
    }
  }

  private void ValueBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
    if (sender is TextBox tb && !tb.IsKeyboardFocusWithin) {
      e.Handled = true;
      tb.Focus();
      tb.SelectAll();
    }
  }

  public event PropertyChangedEventHandler? PropertyChanged;
  private void OnPropertyChanged([CallerMemberName] string? name = null)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
