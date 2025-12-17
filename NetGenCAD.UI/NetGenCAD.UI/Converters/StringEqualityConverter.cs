using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace NetGenCAD.UI.Converters
{
    public class StringEqualityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            value = value?.ToString();
            if (value is string strValue && parameter is string paramValue)
            {
                return strValue.Equals(paramValue, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}
