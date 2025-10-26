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
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia;

namespace NetScad.UI.Views;

public partial class ScadObjectView : UserControl, INotifyPropertyChanged
{
    private ScadObjectViewModel ViewModel => (ScadObjectViewModel)DataContext!;
    private Window? _parentWindow;
    private IDisposable? _clientSizeObserver;
    
    // Adjust based on your needs:
    // - 1200: Wraps earlier for smaller screens
    // - 1400: Current setting (good for most laptops)
    // - 1600: Only wraps for tablet/small screens
    private const double WRAP_THRESHOLD_WIDTH = 1440;

    public ScadObjectView()
    {
        InitializeComponent();
        DataContext = App.Host?.Services.GetRequiredService<ScadObjectViewModel>();

        // Wait for the control to be attached to the visual tree to access the parent window
        this.AttachedToVisualTree += ScadObjectView_AttachedToVisualTree;
    }

    private void ScadObjectView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // Get the parent window
        _parentWindow = this.FindAncestorOfType<Window>();
        
        if (_parentWindow != null)
        {
            // Subscribe to ClientSize changes using Avalonia's property system
            _clientSizeObserver = _parentWindow.GetObservable(Window.ClientSizeProperty)
                .Subscribe(size =>
                {
                    Console.WriteLine($"Window Width: {size.Width}");
                    AdjustLayoutBasedOnWindowWidth(size.Width);
                });
            
            // Set initial layout based on current window width
            AdjustLayoutBasedOnWindowWidth(_parentWindow.ClientSize.Width);
        }
    }

    private void ParentWindow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Window.ClientSize) && _parentWindow != null)
        {
            var windowWidth = _parentWindow.ClientSize.Width;
            Console.WriteLine($"Window Width: {windowWidth}");
            AdjustLayoutBasedOnWindowWidth(windowWidth);
        }
    }

    private void AdjustLayoutBasedOnWindowWidth(double windowWidth)
    {
        if (windowWidth < WRAP_THRESHOLD_WIDTH)
        {
            // Switch to vertical stacking when width is too small
            AdjustLayoutForNarrowScreen();
        }
        else
        {
            // Use side-by-side layout for wider screens
            AdjustLayoutForWideScreen();
        }
    }

    private async void DeleteSolidButton_Click(object? sender, RoutedEventArgs e)
    {
        // Check each DataGrid for a selected item
        object? selectedItem = SolidDataGrid.SelectedItem
                            ?? SolidDataGridImperial.SelectedItem;

        if (selectedItem != null)
        {
            await ViewModel.DeleteSelectedItemAsync(selectedItem);
        }
    }

    private async void DeleteModuleButton_Click(object? sender, RoutedEventArgs e)
    {
        // Check each DataGrid for a selected item
        object? selectedItem = ModulesIntersectionDataGrid.SelectedItem
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
        var excludedColumns = new[] { "Id", "ModuleDimensionsId", "OpenSCAD_DecimalPlaces", "CreatedAt", "Resolution", "OSCADMethod", "AxisDimensionsId", "AxisOSCADMethod", "Round_r_MM", "Round_r_IN", "Round_h_MM", "Round_h_IN",
            "Length_IN", "Width_IN", "Height_IN", "Thickness_IN", "XOffset_IN", "YOffset_IN", "ZOffset_IN", "Material", "Radius_IN", "Radius1_IN", "Radius2_IN", "CylinderHeight_IN", "Name", "Radius1_MM", "Radius2_MM"  };

        if (excludedColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }


            if (ShouldExcludePropertyWithAllZeros(e.PropertyName))
            {
                e.Cancel = true;
                return;
            }


        // Dictionary for custom headers to make them more user-friendly (Metric - abbreviated)
        var columnHeaders = new Dictionary<string, string>
        {
            { "Length_MM", "L (mm)" },
            { "Width_MM", "W (mm)" },
            { "Height_MM", "H (mm)" },
            { "Thickness_MM", "T (mm)" },
            { "Radius_MM", "R (mm)" },
            { "Radius1_MM", "R1 (mm)" },
            { "Radius2_MM", "R2 (mm)" },
            { "CylinderHeight_MM", "Cyl H (mm)" },
            { "XOffset_MM", "X (mm)" },
            { "YOffset_MM", "Y (mm)" },
            { "ZOffset_MM", "Z (mm)" },
            { "SolidType", "Solid Type" },
            { "XRotate", "X°" },
            { "YRotate", "Y°" },
            { "ZRotate", "Z°" },
            { "OperationType", "Apply To" },
            { "Description", "Description" },
            { "ModuleName", "Module Name" },
            { "Name", "Object Name" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }

        // Make SolidType the first column and set it to semibold
        if (e.PropertyName == "SolidType")
        {
            e.Column.DisplayIndex = 0;
            if (e.Column is DataGridTextColumn textColumn)
            {
                textColumn.FontWeight = FontWeight.SemiBold;
            }
        }
    }

    /// <summary>
    /// AOT-safe method to check if a property should be excluded because all values are 0.
    /// Uses explicit property checks instead of reflection.
    /// </summary>
    private bool ShouldExcludePropertyWithAllZeros(string propertyName)
    {
        if (ViewModel?.SolidDimensions == null || !ViewModel.SolidDimensions.Any())
            return false;

        // Use switch expression with explicit property access (AOT-safe)
        return propertyName switch
        {
            // Rotation properties
            "XRotate" => ViewModel.SolidDimensions.All(s => Math.Abs(s.XRotate) == 0.0),
            "YRotate" => ViewModel.SolidDimensions.All(s => Math.Abs(s.YRotate) == 0.0),
            "ZRotate" => ViewModel.SolidDimensions.All(s => Math.Abs(s.ZRotate) == 0.0),
            
            // Offset properties (Metric)
            "XOffset_MM" => ViewModel.SolidDimensions.All(s => Math.Abs(s.XOffset_MM) == 0.0),
            "YOffset_MM" => ViewModel.SolidDimensions.All(s => Math.Abs(s.YOffset_MM) == 0.0),
            "ZOffset_MM" => ViewModel.SolidDimensions.All(s => Math.Abs(s.ZOffset_MM) == 0.0),
            
            // Offset properties (Imperial)
            "XOffset_IN" => ViewModel.SolidDimensions.All(s => Math.Abs(s.XOffset_IN) == 0.0),
            "YOffset_IN" => ViewModel.SolidDimensions.All(s => Math.Abs(s.YOffset_IN) == 0.0),
            "ZOffset_IN" => ViewModel.SolidDimensions.All(s => Math.Abs(s.ZOffset_IN) == 0.0),
            
            // Thickness
            "Thickness_MM" => ViewModel.SolidDimensions.All(s => Math.Abs(s.Thickness_MM) == 0.0),
            "Thickness_IN" => ViewModel.SolidDimensions.All(s => Math.Abs(s.Thickness_IN) == 0.0),
            
            // Optional radius properties
            "Radius1_MM" => ViewModel.SolidDimensions.All(s => Math.Abs(s.Radius1_MM) == 0.0),
            "Radius1_IN" => ViewModel.SolidDimensions.All(s => Math.Abs(s.Radius1_IN) == 0.0),
            "Radius2_MM" => ViewModel.SolidDimensions.All(s => Math.Abs(s.Radius2_MM) == 0.0),
            "Radius2_IN" => ViewModel.SolidDimensions.All(s => Math.Abs(s.Radius2_IN) == 0.0),
            
            // Rounding properties
            "Round_r_MM" => ViewModel.SolidDimensions.All(s => Math.Abs(s.Round_r_MM) == 0.0),
            "Round_h_MM" => ViewModel.SolidDimensions.All(s => Math.Abs(s.Round_h_MM) == 0.0),
            "Round_r_IN" => ViewModel.SolidDimensions.All(s => Math.Abs(s.Round_r_IN) == 0.0),
            "Round_h_IN" => ViewModel.SolidDimensions.All(s => Math.Abs(s.Round_h_IN) == 0.0),
            
            // Default: don't exclude
            _ => false
        };
    }

    private void DataGrid_AutoGeneratingColumnObjectImperial(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display - Hide Metric columns for Imperial view
        var excludedColumns = new[] { "Id", "ModuleDimensionsId", "OpenSCAD_DecimalPlaces", "CreatedAt", "Resolution", "OSCADMethod", "AxisDimensionsId", "AxisOSCADMethod", "Round_r_MM", "Round_r_IN", "Round_h_MM", "Round_h_IN",
            "Length_MM", "Width_MM", "Height_MM", "Thickness_MM", "XOffset_MM", "YOffset_MM", "ZOffset_MM", "Material", "Radius_MM", "Radius1_MM", "Radius2_MM", "CylinderHeight_MM", "Name", "Radius1_IN", "Radius2_IN" };

        if (excludedColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

            if (ShouldExcludePropertyWithAllZeros(e.PropertyName))
            {
                e.Cancel = true;
                return;
            }

        // Dictionary for custom headers to make them more user-friendly (Imperial - abbreviated)
        var columnHeaders = new Dictionary<string, string>
        {
            { "Length_IN", "L (in)" },
            { "Width_IN", "W (in)" },
            { "Height_IN", "H (in)" },
            { "Thickness_IN", "T (in)" },
            { "Radius_IN", "R (in)" },
            { "Radius1_IN", "R1 (in)" },
            { "Radius2_IN", "R2 (in)" },
            { "CylinderHeight_IN", "Cyl H (in)" },
            { "XOffset_IN", "X (in)" },
            { "YOffset_IN", "Y (in)" },
            { "ZOffset_IN", "Z (in)" },
            { "XRotate", "X°" },
            { "YRotate", "Y°" },
            { "ZRotate", "Z°" },
            { "OperationType", "Apply To" },
            { "Description", "Description" },
            { "SolidType", "Solid Type" },
            { "ModuleName", "Module Name" },
            { "Name", "Object Name" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }

        // Make SolidType the first column and set it to semibold
        if (e.PropertyName == "SolidType")
        {
            e.Column.DisplayIndex = 0;
            if (e.Column is DataGridTextColumn textColumn)
            {
                textColumn.FontWeight = FontWeight.SemiBold;
            }
        }
    }

    private void SolidDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Handle selection from either Metric or Imperial DataGrid
        var dataGrid = sender as DataGrid;
        if (dataGrid?.SelectedItem is SolidDimensions selected)
        {
            // Populate ViewModel properties
            ViewModel.LengthMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Length_MM : selected.Length_IN;
            ViewModel.WidthMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Width_MM : selected.Width_IN;
            ViewModel.HeightMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Height_MM : selected.Height_IN;
            ViewModel.ThicknessMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Thickness_MM : selected.Thickness_IN;
            ViewModel.RadiusMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Radius_MM : selected.Radius_IN;
            ViewModel.Radius1MM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Radius1_MM : selected.Radius1_IN;
            ViewModel.Radius2MM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.Radius2_MM : selected.Radius2_IN;
            ViewModel.CylinderHeightMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.CylinderHeight_MM : selected.CylinderHeight_IN;
            ViewModel.XOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.XOffset_MM : selected.XOffset_IN;
            ViewModel.YOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.YOffset_MM : selected.YOffset_IN;
            ViewModel.ZOffsetMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.ZOffset_MM : selected.ZOffset_IN;
            ViewModel.XRotate = selected.XRotate;  // Add rotation
            ViewModel.YRotate = selected.YRotate;
            ViewModel.ZRotate = selected.ZRotate;
            ViewModel.SelectedSolidType = selected.SolidType!;
            ViewModel.SelectedFilament = Enum.Parse<FilamentType>(selected.Material!, ignoreCase: true);
            ViewModel.Name = selected.Name;
            ViewModel.Description = selected.Description ?? string.Empty;
            ViewModel.SelectedOperationType = Enum.Parse<OperationType>(selected.OperationType, ignoreCase: true);

            // Highlight corresponding module row based on ModuleName
            if (!string.IsNullOrEmpty(selected.ModuleName))
            {
                HighlightModuleRow(selected.ModuleName);
            }
        }
    }

    private void DataGrid_AutoGeneratingColumnModule(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display for ModuleDimensions
        var excludedColumns = new[] { "Id", "CreatedAt", "XOffset_MM", "YOffset_MM", "ZOffset_MM", "XOffset_IN", "YOffset_IN", "ZOffset_IN", "OSCADMethod", "ObjectDescription", "XOffset_MM", "YOffset_MM", "ZOffset_MM", "XRotate", "YRotate", "ZRotate", "IncludeMethod" };

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

        // Make ModuleType the first column and set it to semibold
        if (e.PropertyName == "ModuleType")
        {
            e.Column.DisplayIndex = 0;
            if (e.Column is DataGridTextColumn textColumn)
            {
                textColumn.FontWeight = FontWeight.SemiBold;
            }
        }
    }

    private async void ClearButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.ClearInputsAsync();

    private async void CreateButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreateObjectAsync();

    private async void ClearObjectButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.ClearObjectAsync();

    private async void CreateUnionButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreateUnionModuleAsync();

    private async void CreateDifferenceButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreateDifferenceModuleAsync();

    private async void CreateIntersectionButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreateIntersectionModuleAsync();

    private async void ObjectToScadFilesButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.ObjectToScadFilesAsync();

    private async void ApplyAxisButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreateAxisAsync();

    private async void GetObjectButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.GetDimensionsPartAsync();

    private async void UpdateAxisPositionButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.UpdateAxisTranslateAsync();

    private async void ChangeAxisButton_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel.AxisStored = false;
        ViewModel.AxesSelectEnabled = true;
    }

    private void HighlightModuleRow(string moduleName)
    {
        // Clear all previous selections first
        ModulesUnionDataGrid.SelectedItem = null;
        ModulesDifferenceDataGrid.SelectedItem = null;
        ModulesIntersectionDataGrid.SelectedItem = null;

        // Find and select the matching module in the appropriate DataGrid
        var moduleInUnion = ViewModel.ModuleDimensionsUnions?.FirstOrDefault(m => m.Name == moduleName);
        if (moduleInUnion != null)
        {
            ModulesUnionDataGrid.SelectedItem = moduleInUnion;
            ModulesUnionDataGrid.ScrollIntoView(moduleInUnion, null);
            return;
        }

        var moduleInDifference = ViewModel.ModuleDimensionsDifferences?.FirstOrDefault(m => m.Name == moduleName);
        if (moduleInDifference != null)
        {
            ModulesDifferenceDataGrid.SelectedItem = moduleInDifference;
            ModulesDifferenceDataGrid.ScrollIntoView(moduleInDifference, null);
            return;
        }

        var moduleInIntersection = ViewModel.ModuleDimensionsIntersections?.FirstOrDefault(m => m.Name == moduleName);
        if (moduleInIntersection != null)
        {
            ModulesIntersectionDataGrid.SelectedItem = moduleInIntersection;
            ModulesIntersectionDataGrid.ScrollIntoView(moduleInIntersection, null);
        }
    }

    private void AdjustLayoutForNarrowScreen()
    {
        // Find the ScrollViewer by name
        var solidsSection = this.FindControl<ScrollViewer>("SolidsSection");
        if (solidsSection != null)
        {
            // Move Solids to bottom row (row 5), spanning both columns
            Grid.SetRow(solidsSection, 5);
            Grid.SetColumn(solidsSection, 0);
            Grid.SetColumnSpan(solidsSection, 2);
            Grid.SetRowSpan(solidsSection, 1);
        }
    }

    private void AdjustLayoutForWideScreen()
    {
        // Find the ScrollViewer by name
        var solidsSection = this.FindControl<ScrollViewer>("SolidsSection");
        if (solidsSection != null)
        {
            // Restore Solids to right column (column 1)
            Grid.SetRow(solidsSection, 0);
            Grid.SetColumn(solidsSection, 1);
            Grid.SetRowSpan(solidsSection, 5);
            Grid.SetColumnSpan(solidsSection, 1);
        }
    }

    // Clean up event subscription when control is unloaded
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        // Dispose the observable subscription
        _clientSizeObserver?.Dispose();
        _clientSizeObserver = null;
        _parentWindow = null;
    }
}