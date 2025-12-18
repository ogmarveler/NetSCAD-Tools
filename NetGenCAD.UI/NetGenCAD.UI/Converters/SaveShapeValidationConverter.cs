using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace NetGenCAD.UI.Converters
{
    public class SaveShapeValidationConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // Required bindings: 
            // 0: PolyhedronDimensionsPoints (collection)
            // 1: PolyhedronDimensionsFaces (collection)
            // 2: ShapeScad (string)

            if (values == null || values.Count < 3)
                return false;

            // Check if points collection has items
            bool hasPoints = values[0] is ICollection pointsCollection && pointsCollection.Count > 0;

            // Check if faces collection has items
            bool hasFaces = values[1] is ICollection facesCollection && facesCollection.Count > 0;

            // Check if ShapeScad has content
            bool hasShapeScad = values[2] is string shapeScad && !string.IsNullOrWhiteSpace(shapeScad);

            // All three conditions must be true
            return hasPoints && hasFaces && hasShapeScad;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
