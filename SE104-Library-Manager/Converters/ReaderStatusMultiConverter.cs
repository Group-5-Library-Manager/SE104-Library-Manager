using System;
using System.Globalization;
using System.Windows.Data;

namespace SE104_Library_Manager.Converters
{
    public class ReaderStatusMultiConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is DateOnly ngayLapThe && values[1] is int thoiHan)
            {
                var now = DateOnly.FromDateTime(DateTime.Now);
                var expired = ngayLapThe.AddMonths(thoiHan) < now;
                return expired ? "Hết hạn" : "Còn hạn";
            }
            return "";
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}