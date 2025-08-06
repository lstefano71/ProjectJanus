using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace Janus.App
{
  public class LogLevelSelectedConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is ObservableCollection<string> selectedLevels && parameter is string level && selectedLevels.Contains(level);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var isChecked = value as bool?;
      var level = parameter as string;
      _ = targetType == typeof(ObservableCollection<string>) ? value as ObservableCollection<string> : null;
      // This is handled in the ViewModel via binding, so no need to implement here
      return Binding.DoNothing;
    }
  }
}
