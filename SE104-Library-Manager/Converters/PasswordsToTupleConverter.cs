using SE104_Library_Manager.Views;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace SE104_Library_Manager.Converters;

public class PasswordsToTupleConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var pwd1 = values[0] as PasswordBox;
        var pwd2 = values[1] as PasswordBox;
        var addStaffWindow = values[2] as AddStaffWindow;

        return Tuple.Create(pwd1, pwd2, addStaffWindow);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
