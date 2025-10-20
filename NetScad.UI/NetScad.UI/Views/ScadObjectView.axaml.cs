using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using NetScad.Core.Material;
using NetScad.Core.Primitives;
using NetScad.Designer.Repositories;
using NetScad.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static NetScad.Core.Measurements.Selector;

namespace NetScad.UI.Views;

public partial class ScadObjectView : UserControl, INotifyPropertyChanged
{
    private ScadObjectViewModel ViewModel => (ScadObjectViewModel)DataContext!;

    public ScadObjectView()
    {
        InitializeComponent();
        DataContext = App.Host?.Services.GetRequiredService<ScadObjectViewModel>();
    }

    private async void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        // Check each DataGrid for a selected item
        object? selectedItem = CubeDataGrid.SelectedItem
                            ?? CylinderDataGrid.SelectedItem
                            ?? ModulesUnionDataGrid.SelectedItem
                            ?? ModulesDifferenceDataGrid.SelectedItem;

        if (selectedItem != null)
        {
            await ViewModel.DeleteSelectedItemAsync(selectedItem);
        }
    }

    private void DataGrid_AutoGeneratingColumnObject(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display
        var excludedColumns = new[] { "Id", "OpenSCAD_DecimalPlaces", "CreatedAt", "Resolution", "OSCADMethod", "AxisDimensionsId", "AxisOSCADMethod", "Round_r_MM", "Round_r_IN", "Round_h_MM", "Round_h_IN" };

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
            { "XOffset_MM", "X (mm)" },
            { "YOffset_MM", "Y (mm)" },
            { "ZOffset_MM", "Z (mm)" },
            { "Round_r_MM", "Rounded (mm)" },
            { "Length_IN", "Length (in)" },
            { "Width_IN", "Width (in)" },
            { "Height_IN", "Height (in)" },
            { "Thickness_IN", "Thickness (in)" },
            { "XOffset_IN", "X (in)" },
            { "YOffset_IN", "Y (in)" },
            { "ZOffset_IN", "Z (in)" },
            { "Round_r_IN", "Rounded (in)" },
            { "OperationType", "Action" },
            { "Description", "Cube Description" },
            { "Name", "Object Name" },

        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }
    }

    private void CubeDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CubeDataGrid.SelectedItem is OuterDimensions selected)
        {
            // Populate ViewModel properties
            ViewModel.LengthMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Length_MM : selected.Length_IN;
            ViewModel.WidthMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Width_MM : selected.Width_IN;
            ViewModel.HeightMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Height_MM : selected.Height_IN;
            ViewModel.ThicknessMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Thickness_MM : selected.Thickness_IN;
            ViewModel.XOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.XOffset_MM : selected.XOffset_IN;
            ViewModel.YOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.YOffset_MM : selected.YOffset_IN; 
            ViewModel.ZOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.ZOffset_MM : selected.ZOffset_IN;
            ViewModel.SelectedFilament = Enum.Parse<FilamentType>(selected.Material!, ignoreCase: true);
            ViewModel.Name = selected.Name;
            ViewModel.Description = selected.Description ?? string.Empty;
            ViewModel.SelectedOperationType = Enum.Parse<OperationType>(selected.OperationType, ignoreCase: true);
        }
    }

    private void DataGrid_AutoGeneratingColumnModule(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display for ModuleDimensions
        var excludedColumns = new[] { "Id", "CreatedAt", "XOffset_MM", "YOffset_MM", "ZOffset_MM", "XOffset_IN", "YOffset_IN", "ZOffset_IN", "OSCADMethod", "ObjectDescription" };

        if (excludedColumns.Contains(e.PropertyName))
        {   
            e.Cancel = true;
            return;
        }

        // Dictionary for custom headers
        var columnHeaders = new Dictionary<string, string>
        {
            { "ModuleType", "Module Type" },
            { "SolidType", "Solid Type" },
            { "ObjectName", "Object Name" },
            { "ObjectDescription", "Description" },
            { "Name", "OpenSCAD Call Method" },
            { "XOffset_MM", "X (mm)" },
            { "YOffset_MM", "Y (mm)" },
            { "ZOffset_MM", "Z (mm)" },
            { "XOffset_IN", "X (in)" },
            { "YOffset_IN", "Y (in)" },
            { "ZOffset_IN", "Z (in)" },
            { "IncludeMethod", "Include Method" }
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }
    }

    private void DataGrid_AutoGeneratingColumnCylinder(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display
        var excludedColumns = new[] { "Id", "OpenSCAD_DecimalPlaces", "CreatedAt", "Resolution", "OSCADMethod", "AxisDimensionsId", "AxisOSCADMethod"  };

        if (excludedColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        // Dictionary for custom headers to make them more user-friendly
        var columnHeaders = new Dictionary<string, string>
        {
            { "Radius_MM", "Radius (mm)" },
            { "Radius1_MM", "Radius 1 (mm)" },
            { "Radius2_MM", "Radius 2 (mm)" },
            { "Height_MM", "Height (mm)" },
            { "XOffset_MM", "X (mm)" },
            { "YOffset_MM", "Y (mm)" },
            { "ZOffset_MM", "Z (mm)" },
            { "Radius_IN", "Radius (in)" },
            { "Radius1_IN", "Radius 1 (in)" },
            { "Radius2_IN", "Radius 2 (in)" },
            { "Height_IN", "Height (in)" },
            { "XOffset_IN", "X (in)" },
            { "YOffset_IN", "Y (in)" },
            { "ZOffset_IN", "Z (in)" },
            { "AxisOSCADMethod", "Axis" },
            { "OperationType", "Action" },
            { "Description", "Cylinder Description" },
            { "Name", "Object Name" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }
    }

    private void CylinderDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CylinderDataGrid.SelectedItem is CylinderDimensions selected)
        {
            // Populate ViewModel properties
            ViewModel.RadiusMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Radius_MM : selected.Radius_IN;
            ViewModel.Radius1MM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Radius1_MM : selected.Radius1_IN;
            ViewModel.Radius2MM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Radius2_MM : selected.Radius2_IN;
            ViewModel.CylinderHeightMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Height_MM : selected.Height_IN;
            ViewModel.XOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.XOffset_MM : selected.XOffset_IN;
            ViewModel.YOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.YOffset_MM : selected.YOffset_IN;
            ViewModel.ZOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.ZOffset_MM : selected.ZOffset_IN;
            ViewModel.SelectedFilament = Enum.Parse<FilamentType>(selected.Material!, ignoreCase: true);
            ViewModel.Name = selected.Name;
            ViewModel.Description = selected.Description ?? string.Empty;
            ViewModel.SelectedOperationType = Enum.Parse<OperationType>(selected.OperationType, ignoreCase: true);
        }
    }

    private async void ClearButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.ClearInputsAsync();

    private async void CreateButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreateObjectAsync();

    private async void ClearObjectButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.ClearObjectAsync();

    private async void CreateUnionButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreateUnionModuleAsync();

    private async void CreateDifferenceButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreateDifferenceModuleAsync();

    private async void ObjectToScadFilesButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.ObjectToScadFilesAsync();

    private async void ApplyAxisButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreateAxisAsync();

    private async void GetObjectButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.GetDimensionsPartAsync();
}