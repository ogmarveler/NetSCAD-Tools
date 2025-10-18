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
        DataContext = App.Host.Services.GetRequiredService<CreateAxesViewModel>();
    }

    // Convert from one unit to another
    private async void _ConvertInputsAsync(object? sender, RoutedEventArgs e)
    {
        await ViewModel.ConvertInputs(ViewModel._decimalPlaces);
    }

    private async void _CreateCustomAxisAsync(object? sender, RoutedEventArgs e)
    {
        await ViewModel.CreateCustomAxisAsync();
    }

    private async void _ClearInputsAsync(object? sender, RoutedEventArgs e)
    {
        await ViewModel.ClearInputs();
    }

    /// <summary>
    /// Handles auto-generating columns for the AxesList DataGrid
    /// Customizes column headers to be more user-friendly
    /// </summary>
    private void DataGrid_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // Customize column headers based on property name
        e.Column.Header = e.PropertyName switch
        {
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.CallingMethod) => "Module Call",
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.Unit) => "Unit Type",
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.Theme) => "Theme",
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.RangeX) => "Range X",
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.RangeY) => "Range Y",
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.RangeZ) => "Range Z",
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.MinX) => "Min X",
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.MaxX) => "Max X",
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.MinY) => "Min Y",
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.MaxY) => "Max Y",
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.MinZ) => "Min Z",
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.MaxZ) => "Max Z",
            nameof(NetScad.Axis.Scad.Models.GeneratedModule.Volume) => "Volume",
            _ => e.PropertyName // Default to property name if not matched
        };

        // Optional: Set column width based on content type
        if (e.PropertyName == nameof(NetScad.Axis.Scad.Models.GeneratedModule.CallingMethod))
        {
            e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Star); // Take remaining space
        }
    }
}