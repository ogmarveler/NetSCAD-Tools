using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using NetScad.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NetScad.UI.Views;

public partial class ScadObjectView : UserControl, INotifyPropertyChanged
{
    private ScadObjectViewModel ViewModel => (ScadObjectViewModel)DataContext!;

    public ScadObjectView()  // ✅ Inject via constructor
    {
        InitializeComponent();
        DataContext = App.Host.Services.GetRequiredService<ScadObjectViewModel>();
    }

    private void DataGrid_AutoGeneratingColumnObject(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display
        var excludedColumns = new[] { "Id", "OpenSCAD_DecimalPlaces", "CreatedAt", "Resolution", "Round_h_MM", "OSCADMethod", "AxisDimensionsId", "AxisOSCADMethod" };

        if (excludedColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        // Dictionary for custom headers to make them more user-friendly
        var columnHeaders = new Dictionary<string, string>
        {
            { "Length_MM", "Length (mm)" },
            { "Width_MM", "Width (mm)" },
            { "Height_MM", "Height (mm)" },
            { "Thickness_MM", "Thickness (mm)" },
            { "Round_r_MM", "Rounded (mm)" },
            { "Length_IN", "Length (in)" },
            { "Width_IN", "Width (in)" },
            { "Height_IN", "Height (in)" },
            { "Thickness_IN", "Thickness (in)" },
            { "Round_r_IN", "Rounded (in)" },
            { "AxisOSCADMethod", "Axis" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }
    }

    private void DataGrid_AutoGeneratingColumnAxis(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display
        var excludedColumns = new[] { "CreatedAt" };

        if (excludedColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        // Dictionary for custom headers to make them more user-friendly
        var columnHeaders = new Dictionary<string, string>
        {
            { "Theme", "Theme" },
            { "Unit", "Unit" },
            { "MinX", $"Min X" },
            { "MaxX", $"Max X" },
            { "MinY", $"Min Y" },
            { "MaxY", $"Max Y" },
            { "MinZ", $"Min Z" },
            { "MaxZ", $"Max Z" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }
    }

    private async void ClearButton_Click(object? sender, RoutedEventArgs e)
    {
        await ViewModel.ClearInputsAsync();
    }

    private async void CreateButton_Click(object? sender, RoutedEventArgs e)
    {
        await ViewModel.CreateObjectAsync();
    }

    private async void ClearObjectButton_Click(object? sender, RoutedEventArgs e)
    {
        await ViewModel.ClearObjectAsync();
    }
}