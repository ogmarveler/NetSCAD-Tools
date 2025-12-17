using Avalonia.Data.Converters;
using NetGenCAD.Core.Primitives;
using System;
using System.Globalization;

namespace NetGenCAD.UI.Converters
{
    public class PolyhedronOperationTypeConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PolyhedronOperationType operationType)
            {
                return operationType == PolyhedronOperationType.Points ? "Create Point" : "Create Face";
            }
            return "Create";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
