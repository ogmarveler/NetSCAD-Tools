using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using NetScad.Core.Measurements;
using NetScad.UI.ViewModels;
using System;
using System.ComponentModel;
using static NetScad.Core.Measurements.Selector;

namespace NetScad.UI.Views;

public partial class CreateAxesView : UserControl, INotifyPropertyChanged
{
    private CreateAxesViewModel ViewModel => (CreateAxesViewModel)DataContext!;
    public CreateAxesView()
    {
        InitializeComponent();
        DataContext = App.Host?.Services.GetRequiredService<CreateAxesViewModel>();
    }

    // Convert from one unit to another
    private async void CreateCustomAxis(object? sender, RoutedEventArgs e) => await ViewModel.CreateCustomAxisAsync();

    private async void ClearInputsAsync(object? sender, RoutedEventArgs e) => await ViewModel.ClearInputs();

    /// <summary>
    /// Handles auto-generating columns for the AxesList DataGrid
    /// Customizes column headers to be more user-friendly
    /// </summary>
    private void DataGrid_AutoGeneratingColumnImperial(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // Customize column headers based on property name
        e.Column.Header = e.PropertyName switch
        {
            nameof(Axis.Scad.Models.GeneratedModule.CallingMethod) => "Module Call",
            nameof(Axis.Scad.Models.GeneratedModule.Unit) => "Unit",
            nameof(Axis.Scad.Models.GeneratedModule.Theme) => "Theme",
            nameof(Axis.Scad.Models.GeneratedModule.RangeX) => "Range X",
            nameof(Axis.Scad.Models.GeneratedModule.RangeY) => "Range Y",
            nameof(Axis.Scad.Models.GeneratedModule.RangeZ) => "Range Z",
            nameof(Axis.Scad.Models.GeneratedModule.MinX) => "Min X",
            nameof(Axis.Scad.Models.GeneratedModule.MaxX) => "Max X",
            nameof(Axis.Scad.Models.GeneratedModule.MinY) => "Min Y",
            nameof(Axis.Scad.Models.GeneratedModule.MaxY) => "Max Y",
            nameof(Axis.Scad.Models.GeneratedModule.MinZ) => "Min Z",
            nameof(Axis.Scad.Models.GeneratedModule.MaxZ) => "Max Z",
            nameof(Axis.Scad.Models.GeneratedModule.Volume) => "Volume",
            _ => e.PropertyName // Default to property name if not matched
        };

        // Set explicit widths for alignment
        e.Column.Width = e.PropertyName switch
        {
            nameof(Axis.Scad.Models.GeneratedModule.CallingMethod) => new DataGridLength(450), // Fixed 450px
            _ => new DataGridLength(1, DataGridLengthUnitType.Auto)
        };
    }

    /// <summary>
    /// Handles auto-generating columns for the AxesList DataGrid
    /// Customizes column headers to be more user-friendly
    /// </summary>
    private void DataGrid_AutoGeneratingColumnMetric(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // Customize column headers based on property name
        e.Column.Header = e.PropertyName switch
        {
            nameof(Axis.Scad.Models.GeneratedModule.CallingMethod) => "Module Call",
            nameof(Axis.Scad.Models.GeneratedModule.Unit) => "Unit",
            nameof(Axis.Scad.Models.GeneratedModule.Theme) => "Theme",
            nameof(Axis.Scad.Models.GeneratedModule.RangeX) => "Range X",
            nameof(Axis.Scad.Models.GeneratedModule.RangeY) => "Range Y",
            nameof(Axis.Scad.Models.GeneratedModule.RangeZ) => "Range Z",
            nameof(Axis.Scad.Models.GeneratedModule.MinX) => "Min X",
            nameof(Axis.Scad.Models.GeneratedModule.MaxX) => "Max X",
            nameof(Axis.Scad.Models.GeneratedModule.MinY) => "Min Y",
            nameof(Axis.Scad.Models.GeneratedModule.MaxY) => "Max Y",
            nameof(Axis.Scad.Models.GeneratedModule.MinZ) => "Min Z",
            nameof(Axis.Scad.Models.GeneratedModule.MaxZ) => "Max Z",
            nameof(Axis.Scad.Models.GeneratedModule.Volume) => "Volume",
            _ => e.PropertyName // Default to property name if not matched
        };

        // Set explicit widths for alignment
        e.Column.Width = e.PropertyName switch
        {
            nameof(Axis.Scad.Models.GeneratedModule.CallingMethod) => new DataGridLength(450), // Fixed 450px
            _ => new DataGridLength(1, DataGridLengthUnitType.Auto)
        };
    }

    /// <summary>
    /// Handles selection changes in the Imperial DataGrid
    /// Populates the input textboxes with the selected row's values
    /// </summary>
    private void DataGridImperial_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem is Axis.Scad.Models.GeneratedModule selected)
        {
            // Set unit system to Imperial
            ViewModel.SelectedUnitValue = UnitSystem.Imperial;

            // Populate ViewModel properties with Imperial values
            ViewModel.MinXValue = selected.MinX;
            ViewModel.MaxXValue = selected.MaxX;
            ViewModel.MinYValue = selected.MinY;
            ViewModel.MaxYValue = selected.MaxY;
            ViewModel.MinZValue = selected.MinZ;
            ViewModel.MaxZValue = selected.MaxZ;
            
            // Parse volume safely - handle null/empty/invalid values
            if (!string.IsNullOrWhiteSpace(selected.Volume) && 
                double.TryParse(selected.Volume.Replace(" in³", "").Trim(), 
                                System.Globalization.NumberStyles.Float, 
                                System.Globalization.CultureInfo.InvariantCulture, 
                                out double volumeIn3))
            {
                ViewModel.TotalCubicVolume = volumeIn3;
                ViewModel.TotalCubicVolumeScale = VolumeConverter.ConvertIn3ToFt3(volumeIn3);
                Console.WriteLine($"Selected Volume (in³): {ViewModel.TotalCubicVolume}, Converted Volume (ft³): {ViewModel.TotalCubicVolumeScale}");
            }
            else
            {
                ViewModel.TotalCubicVolume = 0;
                ViewModel.TotalCubicVolumeScale = 0;
                Console.WriteLine($"Volume parsing failed for value: '{selected.Volume}'");
            }
            
            // Set background type based on Theme
            ViewModel.SelectedBackgroundValue = selected.Theme?.Contains("Light") == true
                ? BackgroundType.Light
                : BackgroundType.Dark;
        }
    }

    /// <summary>
    /// Handles selection changes in the Metric DataGrid
    /// Populates the input textboxes with the selected row's values
    /// </summary>
    private void DataGridMetric_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem is Axis.Scad.Models.GeneratedModule selected)
        {
            // Set unit system to Metric
            ViewModel.SelectedUnitValue = UnitSystem.Metric;

            // Populate ViewModel properties with Metric values
            ViewModel.MinXValue = selected.MinX;
            ViewModel.MaxXValue = selected.MaxX;
            ViewModel.MinYValue = selected.MinY;
            ViewModel.MaxYValue = selected.MaxY;
            ViewModel.MinZValue = selected.MinZ;
            ViewModel.MaxZValue = selected.MaxZ;

            // Parse volume safely - handle null/empty/invalid values
            if (!string.IsNullOrWhiteSpace(selected.Volume) &&
                 double.TryParse(selected.Volume.Replace(" cm³", "").Trim(),
                                 System.Globalization.NumberStyles.Float,
                                 System.Globalization.CultureInfo.InvariantCulture,
                                 out double volumeIn3))
            {
                ViewModel.TotalCubicVolume = volumeIn3;
                ViewModel.TotalCubicVolumeScale = VolumeConverter.ConvertCm3ToM3(volumeIn3);
                Console.WriteLine($"Selected Volume (cm³): {ViewModel.TotalCubicVolume}, Converted Volume (m³): {ViewModel.TotalCubicVolumeScale}");
            }
            else
            {
                ViewModel.TotalCubicVolume = 0;
                ViewModel.TotalCubicVolumeScale = 0;
                Console.WriteLine($"Volume parsing failed for value: '{selected.Volume}'");
            }

            // Set background type based on Theme
            ViewModel.SelectedBackgroundValue = selected.Theme?.Contains("Light") == true
                ? BackgroundType.Light
                : BackgroundType.Dark;
        }
    }
}