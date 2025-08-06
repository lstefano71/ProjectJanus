using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace Janus.App
{
  public class LogLevelSelectedConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var selectedLevels = value as ObservableCollection<string>;
      var level = parameter as string;
      return selectedLevels != null && level != null && selectedLevels.Contains(level);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var isChecked = value as bool?;
      var level = parameter as string;
      var selectedLevels = targetType == typeof(ObservableCollection<string>) ? value as ObservableCollection<string> : null;
      // This is handled in the ViewModel via binding, so no need to implement here
      return Binding.DoNothing;
    }
  }
}
