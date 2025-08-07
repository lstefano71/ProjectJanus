using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Janus.App.Controls;

public partial class TextBoxWithPlaceholder : UserControl
{
  public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
      nameof(Text), typeof(string), typeof(TextBoxWithPlaceholder),
      new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

  public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
      nameof(Placeholder), typeof(string), typeof(TextBoxWithPlaceholder), new PropertyMetadata(string.Empty));

  public string Text {
    get => (string)GetValue(TextProperty);
    set => SetValue(TextProperty, value);
  }

  public string Placeholder {
    get => (string)GetValue(PlaceholderProperty);
    set => SetValue(PlaceholderProperty, value);
  }

  public TextBoxWithPlaceholder()
  {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e)
  {
    UpdatePlaceholderVisibility();
    PART_TextBox.TextChanged += (s, _) => UpdatePlaceholderVisibility();
    PART_TextBox.GotFocus += (s, _) => UpdatePlaceholderVisibility();
    PART_TextBox.LostFocus += (s, _) => UpdatePlaceholderVisibility();
    PART_TextBox.GotKeyboardFocus += PART_TextBox_GotKeyboardFocus;
    PART_TextBox.KeyDown += PART_TextBox_KeyDown;
  }

  private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is TextBoxWithPlaceholder ctrl)
      ctrl.UpdatePlaceholderVisibility();
  }

  private void UpdatePlaceholderVisibility()
  {
    if (string.IsNullOrEmpty(PART_TextBox.Text) && !PART_TextBox.IsKeyboardFocusWithin)
      PART_Placeholder.Visibility = Visibility.Visible;
    else
      PART_Placeholder.Visibility = Visibility.Collapsed;
  }

  private void PART_TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
  {
    PART_TextBox.SelectAll();
  }

  private void PART_TextBox_KeyDown(object sender, KeyEventArgs e)
  {
    // Optionally, raise an event or handle Enter key, etc.
  }
}
