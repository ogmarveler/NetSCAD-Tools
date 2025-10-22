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
                            ?? CubeDataGridImperial.SelectedItem
                            ?? CylinderDataGrid.SelectedItem
                            ?? CylinderDataGridImperial.SelectedItem
                            ?? ModulesUnionDataGrid.SelectedItem
                            ?? ModulesDifferenceDataGrid.SelectedItem;

        if (selectedItem != null)
        {
            await ViewModel.DeleteSelectedItemAsync(selectedItem);
        }
    }

    private void DataGrid_AutoGeneratingColumnObject(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display - Hide Imperial columns for Metric view
        var excludedColumns = new[] { "Id", "OpenSCAD_DecimalPlaces", "CreatedAt", "Resolution", "OSCADMethod", "AxisDimensionsId", "AxisOSCADMethod", "Round_r_MM", "Round_r_IN", "Round_h_MM", "Round_h_IN",
            "Length_IN", "Width_IN", "Height_IN", "Thickness_IN", "XOffset_IN", "YOffset_IN", "ZOffset_IN" };

        if (excludedColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        // Dictionary for custom headers to make them more user-friendly (Metric - abbreviated)
        var columnHeaders = new Dictionary<string, string>
        {
            { "Length_MM", "Len (mm)" },
            { "Width_MM", "W (mm)" },
            { "Height_MM", "Ht (mm)" },
            { "Thickness_MM", "Thk (mm)" },
            { "XOffset_MM", "Xadj (mm)" },
            { "YOffset_MM", "Yadj (mm)" },
            { "ZOffset_MM", "Zadj (mm)" },
            { "XRotate", "X°" },
            { "YRotate", "Y°" },
            { "ZRotate", "Z°" },
            { "OperationType", "Action" },
            { "Description", "Description" },
            { "Name", "Object Name" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }
    }

    private void DataGrid_AutoGeneratingColumnObjectImperial(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display - Hide Metric columns for Imperial view
        var excludedColumns = new[] { "Id", "OpenSCAD_DecimalPlaces", "CreatedAt", "Resolution", "OSCADMethod", "AxisDimensionsId", "AxisOSCADMethod", "Round_r_MM", "Round_r_IN", "Round_h_MM", "Round_h_IN",
            "Length_MM", "Width_MM", "Height_MM", "Thickness_MM", "XOffset_MM", "YOffset_MM", "ZOffset_MM" };

        if (excludedColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        // Dictionary for custom headers to make them more user-friendly (Imperial - abbreviated)
        var columnHeaders = new Dictionary<string, string>
        {
            { "Length_IN", "Len (in)" },
            { "Width_IN", "W (in)" },
            { "Height_IN", "Ht (in)" },
            { "Thickness_IN", "Thk (in)" },
            { "XOffset_IN", "Xadj (in)" },
            { "YOffset_IN", "Yadj (in)" },
            { "ZOffset_IN", "Zadj (in)" },
            { "XRotate", "X°" },
            { "YRotate", "Y°" },
            { "ZRotate", "Z°" },
            { "OperationType", "Action" },
            { "Description", "Description" },
            { "Name", "Object Name" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }
    }

    private void CubeDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Handle selection from either Metric or Imperial DataGrid
        var dataGrid = sender as DataGrid;
        if (dataGrid?.SelectedItem is OuterDimensions selected)
        {
            // Populate ViewModel properties
            ViewModel.LengthMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Length_MM : selected.Length_IN;
            ViewModel.WidthMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Width_MM : selected.Width_IN;
            ViewModel.HeightMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Height_MM : selected.Height_IN;
            ViewModel.ThicknessMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Thickness_MM : selected.Thickness_IN;
            ViewModel.XOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.XOffset_MM : selected.XOffset_IN;
            ViewModel.YOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.YOffset_MM : selected.YOffset_IN; 
            ViewModel.ZOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.ZOffset_MM : selected.ZOffset_IN;
            ViewModel.XRotate = selected.XRotate;  // Add rotation
            ViewModel.YRotate = selected.YRotate;
            ViewModel.ZRotate = selected.ZRotate;
            ViewModel.SelectedFilament = Enum.Parse<FilamentType>(selected.Material!, ignoreCase: true);
            ViewModel.Name = selected.Name;
            ViewModel.Description = selected.Description ?? string.Empty;
            ViewModel.SelectedOperationType = Enum.Parse<OperationType>(selected.OperationType, ignoreCase: true);
        }
    }

    private void DataGrid_AutoGeneratingColumnModule(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display for ModuleDimensions
        var excludedColumns = new[] { "Id", "CreatedAt", "XOffset_MM", "YOffset_MM", "ZOffset_MM", "XOffset_IN", "YOffset_IN", "ZOffset_IN", "OSCADMethod", "ObjectDescription", "XOffset_MM", "YOffset_MM", "ZOffset_MM", "XRotate", "YRotate", "ZRotate" };

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
        // List of columns to exclude from display - Hide Imperial columns for Metric view
        var excludedColumns = new[] { "Id", "OpenSCAD_DecimalPlaces", "CreatedAt", "Resolution", "OSCADMethod", "AxisDimensionsId", "AxisOSCADMethod",
            "Radius_IN", "Radius1_IN", "Radius2_IN", "Height_IN", "XOffset_IN", "YOffset_IN", "ZOffset_IN" };

        if (excludedColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        // Dictionary for custom headers to make them more user-friendly (Metric - abbreviated)
        var columnHeaders = new Dictionary<string, string>
        {
            { "Radius_MM", "R (mm)" },
            { "Radius1_MM", "R1 (mm)" },
            { "Radius2_MM", "R2 (mm)" },
            { "Height_MM", "Ht (mm)" },
            { "XOffset_MM", "Xadj (mm)" },
            { "YOffset_MM", "Yadj (mm)" },
            { "ZOffset_MM", "Zadj (mm)" },
            { "XRotate", "X°" },
            { "YRotate", "Y°" },
            { "ZRotate", "Z°" },
            { "OperationType", "Action" },
            { "Description", "Description" },
            { "Name", "Object Name" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }
    }

    private void DataGrid_AutoGeneratingColumnCylinderImperial(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display - Hide Metric columns for Imperial view
        var excludedColumns = new[] { "Id", "OpenSCAD_DecimalPlaces", "CreatedAt", "Resolution", "OSCADMethod", "AxisDimensionsId", "AxisOSCADMethod",
            "Radius_MM", "Radius1_MM", "Radius2_MM", "Height_MM", "XOffset_MM", "YOffset_MM", "ZOffset_MM" };

        if (excludedColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        // Dictionary for custom headers to make them more user-friendly (Imperial - abbreviated)
        var columnHeaders = new Dictionary<string, string>
        {
            { "Radius_IN", "R (in)" },
            { "Radius1_IN", "R1 (in)" },
            { "Radius2_IN", "R2 (in)" },
            { "Height_IN", "Ht (in)" },
            { "XOffset_IN", "Xadj (in)" },
            { "YOffset_IN", "Yadj (in)" },
            { "ZOffset_IN", "Zadj (in)" },
            { "XRotate", "X°" },
            { "YRotate", "Y°" },
            { "ZRotate", "Z°" },
            { "OperationType", "Action" },
            { "Description", "Description" },
            { "Name", "Object Name" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }
    }

    private void CylinderDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Handle selection from either Metric or Imperial DataGrid
        var dataGrid = sender as DataGrid;
        if (dataGrid?.SelectedItem is CylinderDimensions selected)
        {
            // Populate ViewModel properties
            ViewModel.RadiusMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Radius_MM : selected.Radius_IN;
            ViewModel.Radius1MM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Radius1_MM : selected.Radius1_IN;
            ViewModel.Radius2MM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Radius2_MM : selected.Radius2_IN;
            ViewModel.CylinderHeightMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Height_MM : selected.Height_IN;
            ViewModel.XOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.XOffset_MM : selected.XOffset_IN;
            ViewModel.YOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.YOffset_MM : selected.YOffset_IN;
            ViewModel.ZOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.ZOffset_MM : selected.ZOffset_IN;
            ViewModel.XRotate = selected.XRotate;  // Add rotation
            ViewModel.YRotate = selected.YRotate;
            ViewModel.ZRotate = selected.ZRotate;
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

    private async void UpdateAxisPositionButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.UpdateAxisTranslateAsync();
}