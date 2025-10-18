using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NetScad.UI.Converters
{
    /// <summary>
    /// Multi-value converter that returns true only if all string values are not null or empty.
    /// Used to enable controls when multiple string fields have valid input.
    /// </summary>
    public class MultiStringNotNullOrEmptyConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count == 0)
                return false;

            // Check if all values are non-null, non-empty strings
            return values.All(value =>
                value is string stringValue &&
                !string.IsNullOrEmpty(stringValue));
        }
    }
}
