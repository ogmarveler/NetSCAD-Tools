using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace NetScad.UI.Converters
{
    public class DoubleConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is double d ? d.ToString(CultureInfo.InvariantCulture) : "0";

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str && double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            return 0.0;
        }
    }
}
