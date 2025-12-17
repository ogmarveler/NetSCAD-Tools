using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace NetGenCAD.UI.Converters
{
    public class IntToNullableConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int d && d == 0)
                return null;
            return value?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
                return 0;

            if (int.TryParse(value.ToString(), out int result))
                return result;

            return 0;
        }
    }
}