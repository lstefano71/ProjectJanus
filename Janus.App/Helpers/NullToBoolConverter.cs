using System.Globalization;
using System.Windows.Data;

namespace Janus.App;

public class NullToBoolConverter : IValueConverter {
  public bool Invert { get; set; }
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    bool result = value != null;
    return Invert ? !result : result;
  }
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      => throw new NotImplementedException();
}
