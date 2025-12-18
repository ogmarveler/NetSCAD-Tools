using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using static NetGenCAD.Core.Measurements.Conversion;

namespace NetGenCAD.UI.Converters
{
    /// <summary>
    /// Converts millimeter values to the appropriate unit (mm or inches) based on the selected unit system.
    /// Implements IMultiValueConverter to receive both the MM value and IsMetric boolean.
    /// </summary>
    public class UnitConversionConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // Expected: values[0] = double (MM value), values[1] = bool (IsMetric)
            if (values.Count < 2)
                return "N/A";

            if (values[0] is not double mmValue)
                return "N/A";

            if (values[1] is not bool isMetric)
                return "N/A";

            // Default format string
            string formatString = "{0:F2}";

            // If parameter is provided, use it as the format string
            if (parameter is string paramStr && !string.IsNullOrEmpty(paramStr))
            {
                formatString = paramStr;
            }

            // Convert based on unit system
            if (isMetric)
            {
                // Metric: display as-is in millimeters
                return string.Format(culture, formatString, mmValue);
            }
            else
            {
                // Imperial: convert mm to inches
                double inches = MillimeterToInches(mmValue);
                return string.Format(culture, formatString, inches);
            }
        }

        public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not supported for UnitConversionConverter");
        }
    }
}
