using Avalonia.Data.Converters;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace NetScad.UI.Converters
{
    public class EnumDescriptionConverter : IValueConverter
    {
        [UnconditionalSuppressMessage("Trimming", "IL2075:DynamicallyAccessedMembers", Justification = "Enum field reflection is guaranteed at runtime for enum types")]
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            var type = value.GetType();
            if (!type.IsEnum)
                return value.ToString() ?? string.Empty;

            var fieldName = value.ToString();
            if (string.IsNullOrEmpty(fieldName))
                return string.Empty;

            var field = type.GetField(fieldName);
            if (field == null)
                return fieldName;

            if (field.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute attribute)
            {
                return attribute.Description;
            }

            return fieldName;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 
            throw new NotSupportedException();
    }
}
