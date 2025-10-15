using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace NetScad.UI.Converters
{
    public class DoubleToNullableConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d && d == 0)
                return null;
            return value?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
                return 0.0;

            if (double.TryParse(value.ToString(), out double result))
                return result;

            return 0.0;
        }
    }
}