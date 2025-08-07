using System.Globalization;
using System.Windows.Data;

namespace Janus.App.Helpers;

public class ScanIdCheckedMultiConverter : IMultiValueConverter {
  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
    if (values.Length >= 2 && values[0] is IReadOnlyCollection<int> checkedIds) {
      if (values[1] is int scanId) {
        return checkedIds.Contains(scanId);
      }

      if (values[1] is string scanIdStr && int.TryParse(scanIdStr, out int scanId2)) {
        return checkedIds.Contains(scanId2);
      }
    }
    return false;
  }

  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
    // Not used, handled by command
    [Binding.DoNothing, Binding.DoNothing];
}
