using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using NetGenCAD.Core.Primitives;
using NetGenCAD.Designer.Repositories;
using NetGenCAD.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static NetGenCAD.Core.Measurements.Selector;

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
                //AddViewOscadButtonColumnToPointsDataGrid();
                AddActionButtonColumnToPointsDataGridImperial();
                //AddViewOscadButtonColumnToPointsDataGridImperial();
                _isDataGridPointsLoaded = true;
            }
            if (!_isDataGridFacesLoaded)
            {
                AddActionButtonColumnToFacesDataGrid();
                //AddViewOscadButtonColumnToFacesDataGrid();
                _isDataGridFacesLoaded = true;
            }
        };
    }

    // Helper method to safely retrieve brush resources with fallback
    private IBrush GetBrushResource(string resourceKey)
    {
        var resource = Application.Current?.FindResource(resourceKey);
        
        if (resource is IBrush brush)
        {
            return brush;
        }
        
        // Fallback to default foreground colors based on theme
        return new SolidColorBrush(Color.Parse("#FFFFFF")); // Default to white
    }

    // Calling functions from View to ViewModel
    private void ClearButton_Click(object? sender, RoutedEventArgs e) => ViewModel.ClearInputs();
    private void ClearShapeButton_Click(object? sender, RoutedEventArgs e) => ViewModel.ClearShape();
    private void ImportShapeButton_Click(object? sender, RoutedEventArgs e) => ViewModel.GetDimensionPolyhedronParts();
    private void ViewPointsFacesButton_Click(object? sender, RoutedEventArgs e) => ViewModel.ShowShapeScadCode();
    private async void PreviewShapeButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.ShowShapePreviewAsync();
    private void UpdatePointsFaceIdsButton_Click(object? sender, RoutedEventArgs e) => ViewModel.UpdatePolyhedronIds();
    private async void SaveShapeButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.CreateNewShapeModuleAsync();
    private async void UpdateSolidDimensionsButton_Click(object? sender, RoutedEventArgs e) => await ViewModel.UpdateSolidDimensionsAsync();
    
    // UI Stuff
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
    private async void ApplyAxisButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is ScadShapeViewModel viewModel)
        {
            await viewModel.CreateAxis();
        }
    }

    private void UpdateAxisPositionButton_Click(object? sender, RoutedEventArgs e) => ViewModel.UpdateAxisTranslate();

    private void ChangeAxisButton_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel.AxisStored = false;
        ViewModel.AxesSelectEnabled = true;
    }

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
            { "PointsId", "Point ID" },
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
            { "PointsId", "Point ID" },
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
                    ViewModel.GetDimensionPolyhedronParts(); // Refresh datagrids
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
                    ViewModel.GetDimensionPolyhedronParts(); // Refresh datagrids
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
                    ViewModel.GetDimensionPolyhedronParts(); // Refresh datagrids
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

    private void DataGrid_AutoGeneratingColumnShapeDimensions(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // Set FontSize to 12 for all columns
        if (e.Column is DataGridTextColumn textColumn)
        {
            textColumn.FontSize = 12;
        }

        // Hide internal ID columns
        if (e.PropertyName == "Id")
        {
            e.Cancel = true;
            return;
        }

        // Hide imperial conversion columns in metric view
        if (e.PropertyName == "BoxLength_IN" || e.PropertyName == "BoxWidth_IN" || e.PropertyName == "BoxHeight_IN" ||
            e.PropertyName == "SurfaceArea_IN2" || e.PropertyName == "Volume_IN3" || e.PropertyName == "Description" || e.PropertyName == "CreatedAt" || e.PropertyName == "OSCADMethod")
        {
            e.Cancel = true;
            return;
        }

        // Format headers
        if (e.Column.Header is string header)
        {
            e.Column.Header = header
                .Replace("_MM", " (mm)")
                .Replace("_CM2", " (cm²)")
                .Replace("_CM3", " (cm³)")
                .Replace("_IN", " (in)")
                .Replace("_IN2", " (in²)")
                .Replace("_IN3", " (in³)")
                .Replace("NumberOfVertices", "Vertices")
                .Replace("NumberOfFaces", "Faces")
                .Replace("NumberOfEdges", "Edges")
                .Replace("CreatedAt", "Created")
                .Replace("Volume", "V")
                .Replace("SurfaceArea", "A")
                .Replace("Length", "L")
                .Replace("Width", "W")
                .Replace("Height", "H")
                .Replace("Box", "b");
        }

        // Add action columns at the end
        if (e.PropertyName == "OSCADMethod")
        {
            e.Cancel = true; // Don't show OSCADMethod as regular column
            AddViewOscadButtonColumnToShapeDimensionsDataGrid();
        }
    }

    private void DataGrid_AutoGeneratingColumnShapeDimensionsImperial(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // Set FontSize to 12 for all columns
        if (e.Column is DataGridTextColumn textColumn)
        {
            textColumn.FontSize = 12;
        }

        // Hide internal ID columns
        if (e.PropertyName == "Id")
        {
            e.Cancel = true;
            return;
        }

        // Hide metric conversion columns in imperial view
        if (e.PropertyName == "BoxLength_MM" || e.PropertyName == "BoxWidth_MM" || e.PropertyName == "BoxHeight_MM" ||
            e.PropertyName == "SurfaceArea_CM2" || e.PropertyName == "Volume_CM3" || e.PropertyName == "Description" || e.PropertyName == "CreatedAt" || e.PropertyName == "OSCADMethod")
        {
            e.Cancel = true;
            return;
        }

        // Format headers
        if (e.Column.Header is string header)
        {
            e.Column.Header = header
                .Replace("_IN", " (in)")
                .Replace("_IN2", " (in²)")
                .Replace("_IN3", " (in³)")
                .Replace("_MM", " (mm)")
                .Replace("_CM2", " (cm²)")
                .Replace("_CM3", " (cm³)")
                .Replace("NumberOfVertices", "Vertices")
                .Replace("NumberOfFaces", "Faces")
                .Replace("NumberOfEdges", "Edges")
                .Replace("CreatedAt", "Created")
                .Replace("Volume","V")
                .Replace("SurfaceArea", "A")
                .Replace("Length", "L")
                .Replace("Width", "W")
                .Replace("Height","H")
                .Replace("Box","b");
        }

        // Add action columns at the end
        if (e.PropertyName == "OSCADMethod")
        {
            e.Cancel = true; // Don't show OSCADMethod as regular column
            AddViewOscadButtonColumnToShapeDimensionsDataGridImperial();
        }
    }

    private void AddViewOscadButtonColumnToShapeDimensionsDataGrid()
    {
        var cardForeground = GetBrushResource("CardForeground");

        var actionColumn = new DataGridTemplateColumn
        {
            Header = "Actions",
            CellTemplate = new FuncDataTemplate<ShapeDimensions>((value, _) =>
            {
                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };

                // View OSCAD button
                var viewButton = new Button
                {
                    Width = 32,
                    MinWidth = 32,
                    MaxWidth = 32,
                    MaxHeight = 32,
                    BorderThickness = new Thickness(1),
                    BorderBrush = cardForeground,
                    Content = new PathIcon
                    {
                        Width = 16,
                        Height = 16,
                        Data = Geometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm3.5-9c.83 0 1.5-.67 1.5-1.5S16.33 8 15.5 8 14 8.67 14 9.5s.67 1.5 1.5 1.5zm-7 0c.83 0 1.5-.67 1.5-1.5S9.33 8 8.5 8 7 8.67 7 9.5 7.67 11 8.5 11zm3.5 6.5c2.33 0 4.31-1.46 5.11-3.5H6.89c.8 2.04 2.78 3.5 5.11 3.5z"),
                        Foreground = cardForeground
                    }
                };
                viewButton.Click += (s, e) =>
                {
                    if (ViewModel is ScadShapeViewModel viewModel)
                    {
                        viewModel.ShowShapeOSCADMethod(value);
                    }
                };

                // Delete button
                var deleteButton = new Button
                {
                    Width = 32,
                    MinWidth = 32,
                    MaxWidth = 32,
                    MaxHeight = 32,
                    BorderThickness = new Thickness(1),
                    BorderBrush = cardForeground,
                    Content = new PathIcon
                    {
                        Width = 16,
                        Height = 16,
                        Data = Geometry.Parse("M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-9l-1 1H5v2h14V4z"),
                        Foreground = cardForeground
                    }
                };
                deleteButton.Click += async (s, e) =>
                {
                    if (ViewModel is ScadShapeViewModel viewModel)
                    {
                        await viewModel.DeleteShapeAsync(value);
                    }
                };

                stackPanel.Children.Add(viewButton);
                stackPanel.Children.Add(deleteButton);
                return stackPanel;
            })
        };

        ShapeDimensionsDataGrid.Columns.Add(actionColumn);
    }

    private void AddViewOscadButtonColumnToShapeDimensionsDataGridImperial()
    {
        var cardForeground = GetBrushResource("CardForeground");

        var actionColumn = new DataGridTemplateColumn
        {
            Header = "Actions",
            CellTemplate = new FuncDataTemplate<ShapeDimensions>((value, _) =>
            {
                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };

                // View OSCAD button
                var viewButton = new Button
                {
                    Width = 32,
                    MinWidth = 32,
                    MaxWidth = 32,
                    MaxHeight = 32,
                    BorderThickness = new Thickness(1),
                    BorderBrush = cardForeground,
                    Content = new PathIcon
                    {
                        Width = 16,
                        Height = 16,
                        Data = Geometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm3.5-9c.83 0 1.5-.67 1.5-1.5S16.33 8 15.5 8 14 8.67 14 9.5s.67 1.5 1.5 1.5zm-7 0c.83 0 1.5-.67 1.5-1.5S9.33 8 8.5 8 7 8.67 7 9.5 7.67 11 8.5 11zm3.5 6.5c2.33 0 4.31-1.46 5.11-3.5H6.89c.8 2.04 2.78 3.5 5.11 3.5z"),
                        Foreground = cardForeground
                    }
                };
                viewButton.Click += (s, e) =>
                {
                    if (ViewModel is ScadShapeViewModel viewModel)
                    {
                        viewModel.ShowShapeOSCADMethod(value);
                    }
                };

                // Delete button
                var deleteButton = new Button
                {
                    Width = 32,
                    MinWidth = 32,
                    MaxWidth = 32,
                    MaxHeight = 32,
                    BorderThickness = new Thickness(1),
                    BorderBrush = cardForeground,
                    Content = new PathIcon
                    {
                        Width = 16,
                        Height = 16,
                        Data = Geometry.Parse("M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-9l-1 1H5v2h14V4z"),
                        Foreground = cardForeground
                    }
                };
                deleteButton.Click += async (s, e) =>
                {
                    if (ViewModel is ScadShapeViewModel viewModel)
                    {
                        await viewModel.DeleteShapeAsync(value);
                    }
                };

                stackPanel.Children.Add(viewButton);
                stackPanel.Children.Add(deleteButton);
                return stackPanel;
            })
        };

        ShapeDimensionsDataGridImperial.Columns.Add(actionColumn);
    }
}