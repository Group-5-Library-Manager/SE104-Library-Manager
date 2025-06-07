using System.Globalization;
using System.Windows.Data;

namespace SE104_Library_Manager.Converters;

public class PrefixConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var prefix = parameter as string ?? "";
        return value != null ? $"{prefix}{value}" : "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
