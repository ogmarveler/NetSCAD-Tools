using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace NetScad.UI.Converters
{
    /// <summary>
    /// Converts a collection count to text based on whether the collection is empty or has items
    /// </summary>
    public class CollectionCountToTextConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // Get the first value which should be the collection
            if (values == null || values.Count == 0)
                return "Create Object";

            var value = values[0];

            if (value is ICollection collection)
            {
                // Parse parameter: format is "TextWhenEmpty|TextWhenHasItems"
                if (parameter is string paramStr)
                {
                    // Remove any quotes from the parameter
                    paramStr = paramStr.Trim('\'', '"');

                    var texts = paramStr.Split('|');
                    if (texts.Length == 2)
                    {
                        return collection.Count == 0 ? texts[0] : texts[1];
                    }
                }
            }

            return "Create Object";
        }

        public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
