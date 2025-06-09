using System.Globalization;
using System.Windows.Data;

namespace SE104_Library_Manager.Converters;

public class DateOnlyFormattedConverter : IValueConverter
{
    public string Format { get; set; } = "dd/MM/yyyy";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateOnly dateOnly)
        {
            return dateOnly.ToString(Format, culture);
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && DateOnly.TryParseExact(str, Format, culture, DateTimeStyles.None, out var dateOnly))
        {
            return dateOnly;
        }
        return null;
    }
}
