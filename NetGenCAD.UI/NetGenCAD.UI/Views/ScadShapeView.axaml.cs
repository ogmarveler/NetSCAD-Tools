using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NetGenCAD.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Controls.Templates;
using NetGenCAD.Designer.Repositories;
using static NetGenCAD.Core.Measurements.Selector;
using NetGenCAD.Core.Primitives;

namespace NetGenCAD.UI.Views;

public partial class ScadShapeView : UserControl, INotifyPropertyChanged
{
    private ScadShapeViewModel ViewModel => (ScadShapeViewModel)DataContext!;
    private Window? _parentWindow;
    private IDisposable? _clientSizeObserver;
    private bool _isDataGridPointsLoaded = false;
    private bool _isDataGridFacesLoaded = false;

    // Adjust based on your needs:
    // - 1200: Wraps earlier for smaller screens
    // - 1400: Current setting (good for most laptops)
    // - 1600: Only wraps for tablet/small screens
    private const double WRAP_THRESHOLD_WIDTH = 1400;

    public ScadShapeView()
    {
        InitializeComponent();
        DataContext = App.Services!.GetRequiredService<ScadShapeViewModel>();
        
        // Add action columns to DataGrids after UI is initialized
        this.Loaded += (s, e) =>
        {
            // Ensure it is only added the first time
            if (!_isDataGridPointsLoaded)
            {
                AddActionButtonColumnToPointsDataGrid();
                AddViewOscadButtonColumnToPointsDataGrid();
                AddActionButtonColumnToPointsDataGridImperial();
                AddViewOscadButtonColumnToPointsDataGridImperial();
                _isDataGridPointsLoaded = true;
            }
            if (!_isDataGridFacesLoaded)
            {
                AddActionButtonColumnToFacesDataGrid();
                AddViewOscadButtonColumnToFacesDataGrid();
                _isDataGridFacesLoaded = true;
            }
        };
    }

    private void ClearButton_Click(object? sender, RoutedEventArgs e) => ViewModel.ClearInputs();

    private void ClearShapeButton_Click(object? sender, RoutedEventArgs e) => ViewModel.ClearShape();

    private void ImportShapeButton_Click(object? sender, RoutedEventArgs e) => ViewModel.GetDimensionPolyhedronParts();

    private void AdjustLayoutForNarrowScreen()
    {
        // Find the ScrollViewers by name
        var pointsSection = this.FindControl<ScrollViewer>("PointsSection");
        var facesSection = this.FindControl<ScrollViewer>("FacesSection");
        
        if (pointsSection != null)
        {
            Grid.SetRow(pointsSection, 3);
            Grid.SetColumn(pointsSection, 0);
            Grid.SetColumnSpan(pointsSection, 2);
        }
        
        if (facesSection != null)
        {
            Grid.SetRow(facesSection, 4);
            Grid.SetColumn(facesSection, 0);
            Grid.SetColumnSpan(facesSection, 2);
        }
    }

    private void AdjustLayoutForWideScreen()
    {
        // Find the ScrollViewers by name
        var pointsSection = this.FindControl<ScrollViewer>("PointsSection");
        var facesSection = this.FindControl<ScrollViewer>("FacesSection");
        
        if (pointsSection != null)
        {
            Grid.SetRow(pointsSection, 2);
            Grid.SetColumn(pointsSection, 0);
            Grid.SetColumnSpan(pointsSection, 1);
        }
        
        if (facesSection != null)
        {
            Grid.SetRow(facesSection, 2);
            Grid.SetColumn(facesSection, 0);
            Grid.SetColumnSpan(facesSection, 1);
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

    private async void CreatePolyhedronButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreatePolyhedron();

    // ====== POINTS DataGrid Methods ======
    private void DataGrid_AutoGeneratingColumnPoints(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display - Hide Imperial columns for Metric view
        var excludedColumns = new[] { "Id", "Name", "CreatedAt", "OSCADMethod", "PointX_IN", "PointY_IN", "PointZ_IN", "Face", "FaceId" };

        if (excludedColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        // Dictionary for custom headers to make them more user-friendly (Metric - abbreviated)
        var columnHeaders = new Dictionary<string, string>
        {
            { "PointX_MM", "X (mm)" },
            { "PointY_MM", "Y (mm)" },
            { "PointZ_MM", "Z (mm)" },
            { "PolyhedronOperationType", "Apply To" },
            { "Description", "Description" },
            { "PointsId", "Points ID" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }

        // Set FontSize to 12 for all columns
        if (e.Column is DataGridTextColumn textColumn)
        {
            textColumn.FontSize = 12;
        }

        int displayIndex = 5; // Start after the fixed columns

        switch (e.PropertyName)
        {
            case "PolyhedronOperationType":
                e.Column.DisplayIndex = 2;
                if (e.Column is DataGridTextColumn ptColumn)
                    ptColumn.FontWeight = FontWeight.SemiBold;
                break;
            case "Description":
                e.Column.DisplayIndex = 3;
                break;
            case "PointsId":
                e.Column.DisplayIndex = 4;
                break;
            case "PointX_MM":
                e.Column.DisplayIndex = 5;
                break;
            case "PointY_MM":
                e.Column.DisplayIndex = 6;
                break;
            case "PointZ_MM":
                e.Column.DisplayIndex = 7;
                break;
            default:
                e.Column.DisplayIndex = displayIndex++;
                break;
        }
    }

    private void DataGrid_AutoGeneratingColumnPointsImperial(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display - Hide Metric columns for Imperial view
        var excludedColumns = new[] { "Id", "Name", "CreatedAt", "OSCADMethod", "PointX_MM", "PointY_MM", "PointZ_MM", "Face", "FaceId" };

        if (excludedColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        // Dictionary for custom headers to make them more user-friendly (Imperial - abbreviated)
        var columnHeaders = new Dictionary<string, string>
        {
            { "PointX_IN", "X (in)" },
            { "PointY_IN", "Y (in)" },
            { "PointZ_IN", "Z (in)" },
            { "PolyhedronOperationType", "Apply To" },
            { "Description", "Description" },
            { "PointsId", "Points ID" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }

        // Set FontSize to 12 for all columns
        if (e.Column is DataGridTextColumn textColumn)
        {
            textColumn.FontSize = 12;
        }

        int displayIndex = 5; // Start after the fixed columns

        switch (e.PropertyName)
        {
            case "PolyhedronOperationType":
                e.Column.DisplayIndex = 2;
                if (e.Column is DataGridTextColumn ptColumn)
                    ptColumn.FontWeight = FontWeight.SemiBold;
                break;
            case "Description":
                e.Column.DisplayIndex = 3;
                break;
            case "PointsId":
                e.Column.DisplayIndex = 4;
                break;
            case "PointX_IN":
                e.Column.DisplayIndex = 5;
                break;
            case "PointY_IN":
                e.Column.DisplayIndex = 6;
                break;
            case "PointZ_IN":
                e.Column.DisplayIndex = 7;
                break;
            default:
                e.Column.DisplayIndex = displayIndex++;
                break;
        }
    }

    // ====== FACES DataGrid Methods ======
    private void DataGrid_AutoGeneratingColumnFaces(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // List of columns to exclude from display
        var excludedColumns = new[] { "Id", "CreatedAt", "OSCADMethod", "PointX_MM", "PointX_IN", "PointY_MM", "PointY_IN", "PointZ_MM", "PointZ_IN", "PointsId", "Name" };

        if (excludedColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        // Dictionary for custom headers to make them more user-friendly
        var columnHeaders = new Dictionary<string, string>
        {
            { "Face", "Face Points" },
            { "FaceId", "Face ID" },
            { "PolyhedronOperationType", "Apply To" },
            { "Description", "Description" },
            { "Name", "Polyhedron Name" },
        };

        if (columnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }

        // Set FontSize to 12 for all columns
        if (e.Column is DataGridTextColumn textColumn)
        {
            textColumn.FontSize = 12;
        }

        int displayIndex = 3; // Start after the fixed columns

        switch (e.PropertyName)
        {
            case "PolyhedronOperationType":
                e.Column.DisplayIndex = 2;
                if (e.Column is DataGridTextColumn ptColumn)
                    ptColumn.FontWeight = FontWeight.SemiBold;
                break;
            case "Description":
                e.Column.DisplayIndex = 3;
                break;
            case "FaceId":
                e.Column.DisplayIndex = 4;
                break;
            case "Face":
                e.Column.DisplayIndex = 5;
                break;
            default:
                e.Column.DisplayIndex = displayIndex++;
                break;
        }
    }

    // ====== Selection Changed Handlers ======
    private void PointsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Handle selection from either Metric or Imperial Points DataGrid
        var dataGrid = sender as DataGrid;
        if (dataGrid?.SelectedItem is PolyhedronDimensions selected && selected.PolyhedronOperationType == "Points")
        {
            PopulatePointsFields(selected);
        }
    }

    private void FacesDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Handle selection from Faces DataGrid
        var dataGrid = sender as DataGrid;
        if (dataGrid?.SelectedItem is PolyhedronDimensions selected && selected.PolyhedronOperationType == "Faces")
        {
            PopulateFacesFields(selected);
        }
    }

    private void PopulatePointsFields(PolyhedronDimensions selected)
    {
        // Populate ViewModel properties based on selected unit system
        ViewModel.PointXMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.PointX_MM : selected.PointX_IN;
        ViewModel.PointYMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.PointY_MM : selected.PointY_IN;
        ViewModel.PointZMM = ViewModel.SelectedUnitValue == UnitSystem.Metric ? selected.PointZ_MM : selected.PointZ_IN;
        ViewModel.PointsId = selected.PointsId;
        ViewModel.Name = selected.Name;
        ViewModel.Description = selected.Description ?? string.Empty;
        ViewModel.SelectedPolyhedronOperationType = System.Enum.Parse<PolyhedronOperationType>(selected.PolyhedronOperationType, ignoreCase: true);
    }

    private void PopulateFacesFields(PolyhedronDimensions selected)
    {
        // Populate ViewModel properties for faces
        ViewModel.FaceId = selected.FaceId;
        ViewModel.FacePoints = selected.Face ?? string.Empty;
        ViewModel.Name = selected.Name;
        ViewModel.Description = selected.Description ?? string.Empty;
        ViewModel.SelectedPolyhedronOperationType = System.Enum.Parse<PolyhedronOperationType>(selected.PolyhedronOperationType, ignoreCase: true);
    }

    // ====== Action Button Columns for Points DataGrids ======
    private void AddActionButtonColumnToPointsDataGrid()
    {
        var buttonTemplate = new FuncDataTemplate<object>((item, scope) =>
        {
            var button = new Button
            {
                Content = new PathIcon
                {
                    Data = Geometry.Parse("M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z"),
                    Width = 15,
                    Height = 15
                },
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = Brushes.Transparent,
                Height = 20,
                Width = 20,
                Padding = new Avalonia.Thickness(0),
                Margin = new Avalonia.Thickness(0)
            };

            button.Click += async (s, e) =>
            {
                if (item is PolyhedronDimensions polyhedronItem)
                {
                    await ViewModel.DeleteSelectedItemAsync(polyhedronItem);
                }
            };

            return button;
        });

        var actionColumn = new DataGridTemplateColumn
        {
            Header = "",
            MaxWidth = 40,
            CellTemplate = buttonTemplate,
            CanUserSort = false,
            CanUserResize = false,
            DisplayIndex = 0
        };

        var pointsDataGrid = this.FindControl<DataGrid>("PointsDataGrid");
        if (pointsDataGrid != null)
        {
            pointsDataGrid.Columns.Add(actionColumn);
        }
    }

    private void AddViewOscadButtonColumnToPointsDataGrid()
    {
        var buttonTemplate = new FuncDataTemplate<object>((item, scope) =>
        {
            var button = new Button
            {
                Content = new PathIcon
                {
                    Data = Geometry.Parse("M19,3H14.82C14.4,1.84 13.3,1 12,1C10.7,1 9.6,1.84 9.18,3H5A2,2 0 0,0 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3M12,3A1,1 0 0,1 13,4A1,1 0 0,1 12,5A1,1 0 0,1 11,4A1,1 0 0,1 12,3M7,7H17V5H19V19H5V5H7V7M17,11H7V9H17V11M15,15H7V13H15V15Z"),
                    Width = 15,
                    Height = 15
                },
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = Brushes.Transparent,
                Padding = new Avalonia.Thickness(4),
                MinWidth = 40,
                Height = 28
            };

            button.Click += (s, e) =>
            {
                if (item is PolyhedronDimensions polyhedronItem)
                {
                    ViewModel.ShowOSCADMethod(polyhedronItem);
                }
            };

            return button;
        });

        var viewColumn = new DataGridTemplateColumn
        {
            Header = "",
            Width = new DataGridLength(40),
            CellTemplate = buttonTemplate,
            CanUserSort = false,
            CanUserResize = false,
            DisplayIndex = 1
        };

        var pointsDataGrid = this.FindControl<DataGrid>("PointsDataGrid");
        if (pointsDataGrid != null)
        {
            pointsDataGrid.Columns.Add(viewColumn);
        }
    }

    private void AddActionButtonColumnToPointsDataGridImperial()
    {
        var buttonTemplate = new FuncDataTemplate<object>((item, scope) =>
        {
            var button = new Button
            {
                Content = new PathIcon
                {
                    Data = Geometry.Parse("M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z"),
                    Width = 15,
                    Height = 15
                },
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = Brushes.Transparent,
                Height = 20,
                Width = 20,
                Padding = new Avalonia.Thickness(0),
                Margin = new Avalonia.Thickness(0)
            };

            button.Click += async (s, e) =>
            {
                if (item is PolyhedronDimensions polyhedronItem)
                {
                    await ViewModel.DeleteSelectedItemAsync(polyhedronItem);
                }
            };

            return button;
        });

        var actionColumn = new DataGridTemplateColumn
        {
            Header = "",
            MaxWidth = 40,
            CellTemplate = buttonTemplate,
            CanUserSort = false,
            CanUserResize = false,
            DisplayIndex = 0
        };

        var pointsDataGridImperial = this.FindControl<DataGrid>("PointsDataGridImperial");
        if (pointsDataGridImperial != null)
        {
            pointsDataGridImperial.Columns.Add(actionColumn);
        }
    }

    private void AddViewOscadButtonColumnToPointsDataGridImperial()
    {
        var buttonTemplate = new FuncDataTemplate<object>((item, scope) =>
        {
            var button = new Button
            {
                Content = new PathIcon
                {
                    Data = Geometry.Parse("M19,3H14.82C14.4,1.84 13.3,1 12,1C10.7,1 9.6,1.84 9.18,3H5A2,2 0 0,0 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3M12,3A1,1 0 0,1 13,4A1,1 0 0,1 12,5A1,1 0 0,1 11,4A1,1 0 0,1 12,3M7,7H17V5H19V19H5V5H7V7M17,11H7V9H17V11M15,15H7V13H15V15Z"),
                    Width = 15,
                    Height = 15
                },
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = Brushes.Transparent,
                Padding = new Avalonia.Thickness(4),
                MinWidth = 40,
                Height = 28
            };

            button.Click += (s, e) =>
            {
                if (item is PolyhedronDimensions polyhedronItem)
                {
                    ViewModel.ShowOSCADMethod(polyhedronItem);
                }
            };

            return button;
        });

        var viewColumn = new DataGridTemplateColumn
        {
            Header = "",
            Width = new DataGridLength(40),
            CellTemplate = buttonTemplate,
            CanUserSort = false,
            CanUserResize = false,
            DisplayIndex = 1
        };

        var pointsDataGridImperial = this.FindControl<DataGrid>("PointsDataGridImperial");
        if (pointsDataGridImperial != null)
        {
            pointsDataGridImperial.Columns.Add(viewColumn);
        }
    }

    // ====== Action Button Columns for Faces DataGrid ======
    private void AddActionButtonColumnToFacesDataGrid()
    {
        var buttonTemplate = new FuncDataTemplate<object>((item, scope) =>
        {
            var button = new Button
            {
                Content = new PathIcon
                {
                    Data = Geometry.Parse("M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z"),
                    Width = 15,
                    Height = 15
                },
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = Brushes.Transparent,
                Height = 20,
                Width = 20,
                Padding = new Avalonia.Thickness(0),
                Margin = new Avalonia.Thickness(0)
            };

            button.Click += async (s, e) =>
            {
                if (item is PolyhedronDimensions polyhedronItem)
                {
                    await ViewModel.DeleteSelectedItemAsync(polyhedronItem);
                }
            };

            return button;
        });

        var actionColumn = new DataGridTemplateColumn
        {
            Header = "",
            MaxWidth = 40,
            CellTemplate = buttonTemplate,
            CanUserSort = false,
            CanUserResize = false,
            DisplayIndex = 0
        };

        var facesDataGrid = this.FindControl<DataGrid>("FacesDataGrid");
        if (facesDataGrid != null)
        {
            facesDataGrid.Columns.Add(actionColumn);
        }
    }

    private void AddViewOscadButtonColumnToFacesDataGrid()
    {
        var buttonTemplate = new FuncDataTemplate<object>((item, scope) =>
        {
            var button = new Button
            {
                Content = new PathIcon
                {
                    Data = Geometry.Parse("M19,3H14.82C14.4,1.84 13.3,1 12,1C10.7,1 9.6,1.84 9.18,3H5A2,2 0 0,0 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3M12,3A1,1 0 0,1 13,4A1,1 0 0,1 12,5A1,1 0 0,1 11,4A1,1 0 0,1 12,3M7,7H17V5H19V19H5V5H7V7M17,11H7V9H17V11M15,15H7V13H15V15Z"),
                    Width = 15,
                    Height = 15
                },
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = Brushes.Transparent,
                Padding = new Avalonia.Thickness(4),
                MinWidth = 40,
                Height = 28
            };

            button.Click += (s, e) =>
            {
                if (item is PolyhedronDimensions polyhedronItem)
                {
                    ViewModel.ShowOSCADMethod(polyhedronItem);
                }
            };

            return button;
        });

        var viewColumn = new DataGridTemplateColumn
        {
            Header = "",
            Width = new DataGridLength(40),
            CellTemplate = buttonTemplate,
            CanUserSort = false,
            CanUserResize = false,
            DisplayIndex = 1
        };

        var facesDataGrid = this.FindControl<DataGrid>("FacesDataGrid");
        if (facesDataGrid != null)
        {
            facesDataGrid.Columns.Add(viewColumn);
        }
    }
}