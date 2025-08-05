using System;
using System.Globalization;
using System.Windows.Data;

namespace Janus.App;

public class FirstLineConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            var idx = s.IndexOf('\n');
            if (idx >= 0) return s.Substring(0, idx);
            idx = s.IndexOf('\r');
            if (idx >= 0) return s.Substring(0, idx);
            return s;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
