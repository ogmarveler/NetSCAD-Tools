using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using NetGenCAD.Core.Primitives;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace NetGenCAD.UI.Converters
{
    public class PolyhedronOperationTypeToColorConverter : IValueConverter
    {
        // Cache to store the target control for invalidation
        private static readonly ConditionalWeakTable<object, ThemeChangeSubscription> _subscriptions = [];

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not PolyhedronOperationType operationType || Application.Current == null)
                return null;

            var paramStr = parameter?.ToString() ?? "Foreground";
            var theme = Application.Current.ActualThemeVariant;

            // Determine which resource keys to use based on operation type
            // Points = Gold, Faces = Silver
            var (pointsKey, facesKey) = paramStr switch
            {
                "Foreground" => ("GoldBackground", "SilverBackground"),
                "Background" => ("GoldForeground", "SilverForeground"),
                "BorderBrush" => ("GoldBackground", "SilverBackground"),
                _ => ("GoldBackground", "SilverBackground")
            };

            // Get the appropriate brush from theme resources
            var resourceKey = operationType == PolyhedronOperationType.Points ? pointsKey : facesKey;

            if (Application.Current.TryGetResource(resourceKey, theme, out var resource) && resource is IBrush brush)
            {
                return brush;
            }

            // Fallback colors if resource not found (based on current theme)
            return GetFallbackBrush(operationType, theme?.ToString() == "Dark");
        }

        private static SolidColorBrush GetFallbackBrush(PolyhedronOperationType operationType, bool isDark)
        {
            if (isDark)
            {
                return operationType == PolyhedronOperationType.Points
                    ? new SolidColorBrush(Color.Parse("#8b7500")) // Gold for dark theme
                    : new SolidColorBrush(Color.Parse("#838383")); // Silver for dark theme
            }
            else
            {
                return operationType == PolyhedronOperationType.Points
                    ? new SolidColorBrush(Color.Parse("#ffd700")) // Gold for light theme
                    : new SolidColorBrush(Color.Parse("#efefef")); // Silver for light theme
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
