using Janus.Core;

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Janus.App.Helpers;

public class ScanStatusToBrushConverter : IMultiValueConverter {
  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
    if (values.Length == 0 || values[0] is not ScanStatus status) {
      return Brushes.Gray;
    }

    return status switch {
      ScanStatus.Success => Brushes.Green,
      ScanStatus.Partial => Brushes.Orange,
      ScanStatus.Failed => Brushes.Red,
      _ => Brushes.Gray
    };
  }

  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
      => throw new NotImplementedException();
}