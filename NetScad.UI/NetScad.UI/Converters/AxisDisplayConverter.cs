using Avalonia.Data.Converters;
using NetScad.Axis.Scad.Utility;
using System;
using System.Globalization;

namespace NetScad.UI.Converters
{
    /// <summary>
    /// Converter that formats axis calling methods for display using StringFunctions.FormatAxisDisplay
    /// </summary>
    public class AxisDisplayConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string axisCallingMethod)
            {
                return StringFunctions.FormatAxisDisplay(axisCallingMethod);
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // We don't need to convert back since the ComboBox stores the original CallingMethod value
            throw new NotImplementedException("AxisDisplayConverter does not support ConvertBack");
        }
    }
}
