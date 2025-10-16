using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Globalization;

namespace NetScad.UI.Converters
{
    /// <summary>
    /// Converts a collection count to a boolean value
    /// </summary>
    public class CollectionCountToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ICollection collection)
            {
                // Parse the parameter for minimum count (default is 1)
                int minCount = 1;
                if (parameter is string paramStr && int.TryParse(paramStr, out int parsedCount))
                {
                    minCount = parsedCount;
                }

                return collection.Count >= minCount;
            }

            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
