using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using NetScad.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.VisualTree;
using NetScad.Core.Material;
using NetScad.Core.Primitives;
using NetScad.Designer.Repositories;
using System.Linq;
using static NetScad.Core.Measurements.Selector;
using Avalonia.Styling;

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
    private const double WRAP_THRESHOLD_WIDTH = 1400;

    public ScadObjectView()
    {
        InitializeComponent();
        DataContext = App.Services!.GetRequiredService<ScadObjectViewModel>();

        // Wait for the control to be attached to the visual tree to access the parent window
        this.AttachedToVisualTree += ScadObjectView_AttachedToVisualTree;
        // Add columns after the control is attached (application is initialized)
        AddActionButtonColumnToModuleDataGrid();
        AddViewButtonColumnToModuleDataGrid();
        AddSolidCountColumnToModuleDataGrid();
        AddActionButtonColumnToSolidDataGrid();
        AddActionButtonColumnToSolidDataGridImperial();
    }

    // New method to add the solid count column (non-clickable display only)
    private void AddSolidCountColumnToModuleDataGrid()
    {
        var countTemplate = new FuncDataTemplate<object>((item, scope) =>
        {
            if (item is ModuleDimensions module)
            {
                int count = ViewModel.SolidDimensions.Count(s => s.ModuleDimensionsId == module.Id);
                var textBlock = new TextBlock
                {
                    Text = count.ToString(),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    FontWeight = FontWeight.SemiBold,
                    Margin = new Avalonia.Thickness(5)
                };

                return textBlock;
            }
            
            return new TextBlock 
            { 
                Text = "0",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                FontWeight = FontWeight.SemiBold,
                Margin = new Avalonia.Thickness(5)
            };
        });

        var countColumn = new DataGridTemplateColumn
        {
            Header = "Solids",
            Width = new DataGridLength(90),
            CellTemplate = countTemplate,
            CanUserSort = false,
            CanUserResize = false,
            DisplayIndex = 2 // Place after trash bin and clipboard buttons
        };

        ModulesDataGrid.Columns.Add(countColumn);
    }

    // New method to add the view button column with clipboard icon
    private void AddViewButtonColumnToModuleDataGrid()
    {
        // Define the cell template with a button
        var buttonTemplate = new FuncDataTemplate<object>((item, scope) =>
        {
            var button = new Button
            {
                // Use PathIcon with document/clipboard geometry
                Content = new PathIcon
                {
                    Data = Geometry.Parse("M19,3H14.82C14.4,1.84 13.3,1 12,1C10.7,1 9.6,1.84 9.18,3H5A2,2 0 0,0 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3M12,3A1,1 0 0,1 13,4A1,1 0 0,1 12,5A1,1 0 0,1 11,4A1,1 0 0,1 12,3M7,7H17V5H19V19H5V5H7V7M17,11H7V9H17V11M15,15H7V13H15V15Z"),
                    Width = 15,
                    Height = 15
                },
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = Brushes.Transparent,
                Padding = new Thickness(4),
                MinWidth = 40,
                Height = 28
            };

            // Handle the button click
            button.Click += (s, e) =>
            {
                if (item is ModuleDimensions moduleItem)
                {
                    ViewModel.ShowOSCADMethods(moduleItem);
                }
            };

            return button;
        });

        // Create the template column
        var viewColumn = new DataGridTemplateColumn
        {
            Header = "",
            Width = new DataGridLength(40),
            CellTemplate = buttonTemplate,
            CanUserSort = false,
            CanUserResize = false,
            DisplayIndex = 1 // Second column, after trash bin
        };

        // Add the column to the DataGrid
        ModulesDataGrid.Columns.Add(viewColumn);
    }

    // New method to add the trash bin button column to Module DataGrid
    private void AddActionButtonColumnToModuleDataGrid()
    {
        // Define the cell template with a button
        var buttonTemplate = new FuncDataTemplate<object>((item, scope) =>
        {
            var button = new Button
            {
                // Replace text content with an icon
                Content = new PathIcon
                {
                    Data = Geometry.Parse("M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z"),  // Trash bin icon path
                    Width = 15,  // Adjust size to fit the small button
                    Height = 15
                },
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = Brushes.Transparent,
                Height = 20,  // Smaller height to reduce row height
                Width = 20,  // Fixed width to prevent stretching, matching height for square shape
                FontSize = 10,  // Smaller font
                Padding = new Avalonia.Thickness(0),  // Reduced padding
                Margin = new Avalonia.Thickness(0)  // Reduced margin
            };

            // Handle the button click
            button.Click += async (s, e) =>
            {
                if (item is ModuleDimensions moduleItem)
                {
                    await ViewModel.DeleteSelectedItemAsync(moduleItem);
                }
            };

            return button;
        });

        // Create the template column
        var actionColumn = new DataGridTemplateColumn
        {
            Header = "",  // Column header
            MaxWidth = 40,  // Limit max width to prevent excessive stretching
            CellTemplate = buttonTemplate,
            CanUserSort = false,  // Disable sorting if not needed
            CanUserResize = false,  // Disable resizing if not needed
            DisplayIndex = 0 // Always first column
        };

        // Add the column to the DataGrid
        ModulesDataGrid.Columns.Add(actionColumn);
    }

    // New method to add the button column
    private void AddActionButtonColumnToSolidDataGrid()
    {
        // Define the cell template with a button
        var buttonTemplate = new FuncDataTemplate<object>((item, scope) =>
        {
            var button = new Button
            {
                // Replace text content with an icon
                Content = new PathIcon
                {
                    Data = Geometry.Parse("M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z"),  // Trash bin icon path
                    Width = 15,  // Adjust size to fit the small button
                    Height = 15
                },
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Background = Brushes.Transparent,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Height = 20,
                Width = 20,  // Fixed width to prevent stretching, matching height for square shape
                Padding = new Avalonia.Thickness(0),  // Reduced padding
                Margin = new Avalonia.Thickness(0)  // Reduced margin
            };

            // Handle the button click (replace with your logic)
            button.Click += async (s, e) =>
            {
                if (item is SolidDimensions solidItem)
                {
                    // Example: Call a ViewModel method or perform an action
                    // For instance, populate fields or delete the item
                    await ViewModel.DeleteSelectedItemAsync(solidItem);
                    // Or open an edit dialog, etc.
                }
            };

            return button;
        });

        // Create the template column
        var actionColumn = new DataGridTemplateColumn
        {
            Header = "",  // Column header - empty for icon only
            MaxWidth = 40,  // Limit max width to prevent excessive stretching
            CellTemplate = buttonTemplate,
            CanUserSort = false,  // Disable sorting if not needed
            CanUserResize = false,  // Disable resizing if not needed
            DisplayIndex = 0 // Always first column
        };

        // Add the column to the DataGrid
        SolidDataGrid.Columns.Add(actionColumn);
    }

    // New method to add the button column
    private void AddActionButtonColumnToSolidDataGridImperial()
    {
        // Define the cell template with a button
        var buttonTemplate = new FuncDataTemplate<object>((item, scope) =>
        {
            var button = new Button
            {
                // Replace text content with an icon
                Content = new PathIcon
                {
                    Data = Geometry.Parse("M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z"),  // Trash bin icon path
                    Width = 15,  // Adjust size to fit the small button
                    Height = 15
                },
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,  // Align to the right
                Background = Brushes.Transparent,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Height = 20,  // Smaller height to reduce row height
                Width = 20,  // Fixed width to prevent stretching, matching height for square shape
                FontSize = 10,  // Smaller font
                Padding = new Avalonia.Thickness(0),  // Reduced padding
                Margin = new Avalonia.Thickness(0)  // Reduced margin
            };


            // Handle the button click (replace with your logic)
            button.Click += async (s, e) =>
            {
                if (item is SolidDimensions solidItem)
                {
                    // Example: Call a ViewModel method or perform an action
                    // For instance, populate fields or delete the item
                    await ViewModel.DeleteSelectedItemAsync(solidItem);
                    // Or open an edit dialog, etc.
                }
            };

            return button;
        });

        // Create the template column
        var actionColumn = new DataGridTemplateColumn
        {
            Header = "",  // Column header - empty for icon only
            MaxWidth = 40,  // Limit max width to prevent excessive stretching
            CellTemplate = buttonTemplate,
            CanUserSort = false,  // Disable sorting if not needed
            CanUserResize = false,  // Disable resizing if not needed
            DisplayIndex = 0 // Always first column
        };

        // Add the column to the DataGrid
        SolidDataGridImperial.Columns.Add(actionColumn);
    }

    // Helper method to get theme-aware brush with fallback
    private static IBrush GetThemeBrush(string resourceKey, IBrush fallback)
    {
        if (Application.Current?.TryGetResource(resourceKey, Application.Current.ActualThemeVariant, out var resource) == true && resource is IBrush brush)
        {
            return brush;
        }
        return fallback;
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
        object? selectedItem = ModulesDataGrid.SelectedItem;

        if (selectedItem != null)
        {
            await ViewModel.DeleteSelectedItemAsync(selectedItem);
        }
    }

    private void DataGrid_AutoGeneratingColumnObject(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display - Hide Imperial columns for Metric view
        var excludedColumns = new[] { "Id", "ModuleDimensionsId", "OpenSCAD_DecimalPlaces", "CreatedAt", "Resolution", "OSCADMethod", "AxisDimensionsId", "AxisOSCADMethod", "Round_r_MM", "Round_r_IN", "Round_h_MM", "Round_h_IN",
            "Length_IN", "Width_IN", "Height_IN", "Thickness_IN", "XOffset_IN", "YOffset_IN", "ZOffset_IN", "Material", "Radius_IN", "Radius1_IN", "Radius2_IN", "CylinderHeight_IN", "Name", "Radius1_MM", "Radius2_MM", "Volume_IN3", "ModuleName"  };

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
            { "Volume_CM3", "V (cm³)" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }

        // Make SolidType column appear after the button columns
        if (e.PropertyName == "SolidType")
        {
            e.Column.DisplayIndex = 1; // After trash bin button (index 0)
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
            "Length_MM", "Width_MM", "Height_MM", "Thickness_MM", "XOffset_MM", "YOffset_MM", "ZOffset_MM", "Material", "Radius_MM", "Radius1_MM", "Radius2_MM", "CylinderHeight_MM", "Name", "Radius1_IN", "Radius2_IN", "Volume_CM3", "ModuleName" };

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
            { "Volume_IN3", "V (in³)" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }

        // Make SolidType column appear after the button columns
        if (e.PropertyName == "SolidType")
        {
            e.Column.DisplayIndex = 1; // After trash bin button (index 0)
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

        // Make ModuleType column appear after the button columns
        if (e.PropertyName == "ModuleType")
        {
            e.Column.DisplayIndex = 3; // After trash bin (0), clipboard (1), and solids count (2)
            if (e.Column is DataGridTextColumn textColumn)
            {
                textColumn.FontWeight = FontWeight.SemiBold;
            }
        }
    }

    private async void ClearButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.ClearInputsAsync();

    private async void CreateButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreateObjectAsync();

    private async void ClearObjectButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.ClearObjectAsync();

    private async void CreateModulesButton_Click(object? sender, RoutedEventArgs e)
    {
            await ViewModel.CreateUnionModuleAsync();
            await ViewModel.CreateDifferenceModuleAsync();
            await ViewModel.CreateIntersectionModuleAsync();
    }

    private async void RemoveApplyAxisButton_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel.RemoveAxis = !ViewModel.RemoveAxis;
        await ViewModel.UpdateAxisTranslateAsync();
    }

    private async void ObjectToScadFilesButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.ObjectToScadFilesAsync();

    private async void ApplyAxisButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreateAxisAsync();

    private async void GetObjectButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.GetDimensionsPartAsync();

    private async void UpdateAxisPositionButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.UpdateAxisTranslateAsync();

    private void ChangeAxisButton_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel.AxisStored = false;
        ViewModel.AxesSelectEnabled = true;
    }

    private void HighlightModuleRow(string moduleName)
    {
        // Clear all previous selections first
        ModulesDataGrid.SelectedItem = null;

        // Find and select the matching module in the appropriate DataGrid
        var module = ViewModel.ModuleDimensions?.FirstOrDefault(m => m.Name == moduleName);
        if (module != null)
        {
            ModulesDataGrid.SelectedItem = module;
            ModulesDataGrid.ScrollIntoView(module, null);
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