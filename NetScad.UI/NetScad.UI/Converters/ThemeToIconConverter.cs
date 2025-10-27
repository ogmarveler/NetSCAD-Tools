using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace NetScad.UI.Converters;

public class ThemeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string theme)
        {
            // Return the icon key based on theme
            return theme.Contains("Light", StringComparison.OrdinalIgnoreCase) 
                ? "circle_half_fill_regular"  // Light theme icon
                : "dark_theme_regular";       // Dark theme icon
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
