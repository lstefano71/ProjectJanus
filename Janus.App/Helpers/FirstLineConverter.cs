using System.Globalization;
using System.Windows.Data;

namespace Janus.App;

public class FirstLineConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (value is string s) {
      int idx = s.IndexOf('\n');
      if (idx >= 0) {
        return s[..idx];
      }

      idx = s.IndexOf('\r');
      if (idx >= 0) {
        return s[..idx];
      }

      return s;
    }
    return value;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
