using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using NetScad.UI.ViewModels;
using System.ComponentModel;

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
}