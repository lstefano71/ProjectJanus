using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Janus.App;

public class BooleanToGridLengthConverter : IValueConverter {
  public GridLength ExpandedWidth { get; set; } = new GridLength(220);
  public GridLength CollapsedWidth { get; set; } = new GridLength(0);

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (value is bool b && b) {
      return ExpandedWidth;
    }

    return CollapsedWidth;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
