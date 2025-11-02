using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace NetScad.UI.Converters
{
    public class UnitSystemToColorConverter : IValueConverter
    {
        // Cache to store the target control for invalidation
        private static readonly ConditionalWeakTable<object, ThemeChangeSubscription> _subscriptions = [];

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool isMetric || Application.Current == null)
                return null;

            var paramStr = parameter?.ToString() ?? "Foreground";
            var theme = Application.Current.ActualThemeVariant;

            // Determine which resource keys to use based on unit system
            var (metricKey, imperialKey) = paramStr switch
            {
                "Foreground" => ("BlueBackground", "TealBackground"),
                "Background" => ("BlueForeground", "TealForeground"),
                "BorderBrush" => ("BlueBackground", "TealBackground"),
                _ => ("BlueBackground", "TealBackground")
            };

            // Get the appropriate brush from theme resources
            var resourceKey = isMetric ? metricKey : imperialKey;

            if (Application.Current.TryGetResource(resourceKey, theme, out var resource) && resource is IBrush brush)
            {
                return brush;
            }

            // Fallback colors if resource not found (based on current theme)
            return GetFallbackBrush(isMetric, theme?.ToString() == "Dark");
        }

        private static SolidColorBrush GetFallbackBrush(bool isMetric, bool isDark)
        {
            if (isDark)
            {
                return isMetric
                    ? new SolidColorBrush(Color.Parse("#514e65")) // Blue for dark theme
                    : new SolidColorBrush(Color.Parse("#7a4c5f")); // Teal for dark theme
            }
            else
            {
                return isMetric
                    ? new SolidColorBrush(Color.Parse("#617aaa")) // Blue for light theme
                    : new SolidColorBrush(Color.Parse("#48817c")); // Teal for light theme
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        // Helper class to track theme changes
        private class ThemeChangeSubscription : IDisposable
        {
            private readonly IDisposable? _subscription;

            public ThemeChangeSubscription(Action onThemeChanged)
            {
                if (Application.Current != null)
                {
                    _subscription = Application.Current
                        .GetObservable(Application.ActualThemeVariantProperty)
                        .Subscribe(_ => onThemeChanged());
                }
            }

            public void Dispose()
            {
                _subscription?.Dispose();
            }
        }
    }
}
