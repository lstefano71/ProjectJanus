using Microsoft.Xaml.Behaviors;

using System.Windows;
using System.Windows.Controls;

namespace Janus.App;

// This behavior's ONLY job is to add or remove a GroupStyle from the DataGrid.
public class DataGridGroupStyleBehavior : Behavior<DataGrid> {
  // Dependency Property to HOLD the GroupStyle we want to apply.
  public static readonly DependencyProperty GroupStyleProperty =
      DependencyProperty.Register(nameof(GroupStyle), typeof(GroupStyle), typeof(DataGridGroupStyleBehavior), new PropertyMetadata(null));

  public GroupStyle GroupStyle {
    get => (GroupStyle)GetValue(GroupStyleProperty); set => SetValue(GroupStyleProperty, value);
  }

  // Dependency Property to act as the ON/OFF switch.
  public static readonly DependencyProperty IsEnabledProperty =
      DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(DataGridGroupStyleBehavior), new PropertyMetadata(false, OnIsEnabledChanged));

  public bool IsEnabled {
    get => (bool)GetValue(IsEnabledProperty); set => SetValue(IsEnabledProperty, value);
  }

  // This is called when IsEnabled changes.
  private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var behavior = (DataGridGroupStyleBehavior)d;
    if (behavior.AssociatedObject == null) {
      return;
    }

    behavior.ApplyGroupStyle((bool)e.NewValue);
  }

  // Hook into the DataGrid.
  protected override void OnAttached() {
    base.OnAttached();
    ApplyGroupStyle(IsEnabled);
  }

  // The simplified core logic.
  private void ApplyGroupStyle(bool isEnabled) {
    if (AssociatedObject == null) {
      return;
    }

    // Clear the visual style collection first.
    AssociatedObject.GroupStyle.Clear();

    // If enabled, add the style from our dependency property.
    if (isEnabled && GroupStyle != null) {
      AssociatedObject.GroupStyle.Add(GroupStyle);
    }
  }
}
