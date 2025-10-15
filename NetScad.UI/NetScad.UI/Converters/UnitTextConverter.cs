using Avalonia.Data.Converters;
using System;
using System.Globalization;
using static NetScad.Core.Measurements.Selector;

namespace NetScad.UI.Converters
{
    public class UnitTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is UnitSystem unitSystem)
            {
                var paramStr = parameter?.ToString() ?? "mm";
                return unitSystem == UnitSystem.Metric
                    ? paramStr
                    : paramStr.Replace("mm", "in").Replace("millimeters", "inches");
            }
            return "mm";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
