using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace NetScad.UI.Converters;

public class ModuleTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string moduleType)
            return Brushes.Transparent;

        // Parameter determines if we want Background or Foreground color
        var colorType = parameter?.ToString() ?? "Background";

        return moduleType switch
        {
            "Union" => colorType == "Foreground" 
                ? Application.Current?.FindResource("BlueForeground") as IBrush ?? Brushes.Blue
                : Application.Current?.FindResource("BlueBackground") as IBrush ?? Brushes.LightBlue,
            
            "Difference" => colorType == "Foreground"
                ? Application.Current?.FindResource("TealForeground") as IBrush ?? Brushes.Teal
                : Application.Current?.FindResource("TealBackground") as IBrush ?? Brushes.LightCyan,
            
            "Intersection" => colorType == "Foreground"
                ? Application.Current?.FindResource("GoldForeground") as IBrush ?? Brushes.Gold
                : Application.Current?.FindResource("GoldBackground") as IBrush ?? Brushes.LightGoldenrodYellow,
            
            _ => Brushes.Transparent
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
