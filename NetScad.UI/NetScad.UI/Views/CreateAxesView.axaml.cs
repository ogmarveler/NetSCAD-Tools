using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using NetScad.Core.Measurements;
using NetScad.UI.Converters;
using NetScad.UI.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using static NetScad.Core.Measurements.Selector;

namespace NetScad.UI.Views;

public partial class CreateAxesView : UserControl, INotifyPropertyChanged
{
    private CreateAxesViewModel ViewModel => (CreateAxesViewModel)DataContext!;
    private Window? _parentWindow;
    private IDisposable? _clientSizeObserver;
    private const double WRAP_THRESHOLD_WIDTH = 1400;

    // Track if we've added the button columns
    private bool _metricButtonColumnAdded = false;
    private bool _imperialButtonColumnAdded = false;

    public CreateAxesView()
    {
        InitializeComponent();
        DataContext = App.Services!.GetRequiredService<CreateAxesViewModel>();
        this.AttachedToVisualTree += CreateAxesView_AttachedToVisualTree;

        // Set fixed DataGrid widths after XAML is initialized
        SetDataGridWidths();

        // Subscribe to Loaded events to add button columns after ItemsSource is bound
        var metricDataGrid = this.FindControl<DataGrid>("AxesListMetric");
        var imperialDataGrid = this.FindControl<DataGrid>("AxesListImperial");

        if (metricDataGrid != null)
        {
            metricDataGrid.Loaded += MetricDataGrid_Loaded;
        }

        if (imperialDataGrid != null)
        {
            imperialDataGrid.Loaded += ImperialDataGrid_Loaded;
        }
    }

    private void MetricDataGrid_Loaded(object? sender, RoutedEventArgs e)
    {
        if (sender is DataGrid dataGrid && !_metricButtonColumnAdded)
        {
            AddButtonColumn(dataGrid);
            _metricButtonColumnAdded = true;
        }
    }

    private void ImperialDataGrid_Loaded(object? sender, RoutedEventArgs e)
    {
        if (sender is DataGrid dataGrid && !_imperialButtonColumnAdded)
        {
            AddButtonColumn(dataGrid);
            _imperialButtonColumnAdded = true;
        }
    }

    private void AddButtonColumn(DataGrid dataGrid)
    {
        // Check if we haven't already added it (to prevent duplicates)
        if (dataGrid.Columns.Any(c => c.Header?.ToString() == ""))
            return;

        // Create the button column with template
        var buttonColumn = new DataGridTemplateColumn
        {
            Header = "",
            Width = new DataGridLength(40),
            CanUserResize = false,
            CellTemplate = new FuncDataTemplate<Axis.Scad.Models.GeneratedModule>((module, _) =>
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

                button.Click += (s, e) =>
                {
                    if (module?.CallingMethod != null)
                    {
                        ShowCallingMethodModal(module.CallingMethod);
                    }
                };

                return button;
            })
        };

        // Insert at the beginning
        dataGrid.Columns.Insert(0, buttonColumn);
    }

    private void ShowCallingMethodModal(string? callingMethod)
    {
        if (string.IsNullOrEmpty(callingMethod))
            return;
        
        var sb = new StringBuilder();
        sb.AppendLine("// Custom axis scad file");
        sb.AppendLine("use <Axes/axes.scad>;");
        sb.AppendLine();
        sb.AppendLine("// Translate: move axis around object");
        sb.AppendLine($"translate ([0, 0, 0])  {callingMethod}");

        // Set ViewModel properties for the modal
        ViewModel.ModalTitle = "To Use in Your Main SCAD File";
        ViewModel.ModalContent = sb.ToString();
        ViewModel.IsModalOpen = true;
    }

    private void SetDataGridWidths()
    {
        // Find the DataGrids by name (from XAML)
        var metricDataGrid = this.FindControl<DataGrid>("AxesListMetric");
        var imperialDataGrid = this.FindControl<DataGrid>("AxesListImperial");

        if (metricDataGrid != null)
        {
            metricDataGrid.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            metricDataGrid.MinWidth = 960;
        }

        if (imperialDataGrid != null)
        {
            imperialDataGrid.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            imperialDataGrid.MinWidth = 960;
        }
    }

    private void CreateAxesView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _parentWindow = this.FindAncestorOfType<Window>();

        if (_parentWindow != null)
        {
            _clientSizeObserver = _parentWindow.GetObservable(Window.ClientSizeProperty)
                .Subscribe(size =>
                {
                    Console.WriteLine($"Window Width: {size.Width}");
                    AdjustLayoutBasedOnWindowWidth(size.Width);
                });

            AdjustLayoutBasedOnWindowWidth(_parentWindow.ClientSize.Width);
        }
    }

    private void AdjustLayoutBasedOnWindowWidth(double windowWidth)
    {
        if (windowWidth < WRAP_THRESHOLD_WIDTH)
        {
            AdjustLayoutForNarrowScreen();
        }
        else
        {
            AdjustLayoutForWideScreen();
        }
    }

    // Optional: Adjust width based on window size
    private void AdjustDataGridWidthBasedOnWindow(double windowWidth)
    {
        var metricDataGrid = this.FindControl<DataGrid>("AxesListMetric");
        var imperialDataGrid = this.FindControl<DataGrid>("AxesListImperial");

        // Responsive width based on window size
        double dataGridWidth = windowWidth < 1400 ? 600 : 960;

        if (metricDataGrid != null)
            metricDataGrid.Width = dataGridWidth;

        if (imperialDataGrid != null)
            imperialDataGrid.Width = dataGridWidth;
    }

    private async void CreateCustomAxis(object? sender, RoutedEventArgs e) => await ViewModel.CreateCustomAxisAsync();

    private async void ClearInputsAsync(object? sender, RoutedEventArgs e) => await ViewModel.ClearInputs();

    /// <summary>
    /// MVU-style: Simple column customization without adding custom columns
    /// Avoids stateful column tracking and duplication issues
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private void DataGrid_AutoGeneratingColumnImperial(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // Customize CallingMethod column to show formatted axis display
        if (e.PropertyName == nameof(Axis.Scad.Models.GeneratedModule.CallingMethod))
        {
            // Create a custom DataGridTextColumn with value formatting for "Axis Display"
            var axisDisplayColumn = new DataGridTextColumn
            {
                Header = "Axis Display",
                Width = new DataGridLength(230),
                CanUserResize = false,
                IsReadOnly = true,
                Binding = new Binding(nameof(Axis.Scad.Models.GeneratedModule.CallingMethod))
                {
                    Converter = new AxisDisplayConverter()
                }
            };

            // Replace the auto-generated column with Axis Display
            e.Column = axisDisplayColumn;
            return;
        }

        // Customize headers with unicode symbols
        e.Column.Header = e.PropertyName switch
        {
            nameof(Axis.Scad.Models.GeneratedModule.Unit) => "Unit",
            nameof(Axis.Scad.Models.GeneratedModule.Theme) => "Theme",
            nameof(Axis.Scad.Models.GeneratedModule.RangeX) => "ΔX",
            nameof(Axis.Scad.Models.GeneratedModule.RangeY) => "ΔY",
            nameof(Axis.Scad.Models.GeneratedModule.RangeZ) => "ΔZ",
            nameof(Axis.Scad.Models.GeneratedModule.MinX) => "Xₘᵢₙ",
            nameof(Axis.Scad.Models.GeneratedModule.MaxX) => "Xₘₐₓ",
            nameof(Axis.Scad.Models.GeneratedModule.MinY) => "Yₘᵢₙ",
            nameof(Axis.Scad.Models.GeneratedModule.MaxY) => "Yₘₐₓ",
            nameof(Axis.Scad.Models.GeneratedModule.MinZ) => "Zₘᵢₙ",
            nameof(Axis.Scad.Models.GeneratedModule.MaxZ) => "Zₘₐₓ",
            nameof(Axis.Scad.Models.GeneratedModule.Volume) => "Vol",
            _ => e.PropertyName
        };

        // Auto-size all columns
        e.Column.Width = DataGridLength.Auto;
        e.Column.CanUserResize = false;
    }

    private void DataGrid_AutoGeneratingColumnMetric(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // Same logic for both grids
        DataGrid_AutoGeneratingColumnImperial(sender, e);
    }

    private void DataGridImperial_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem is Axis.Scad.Models.GeneratedModule selected)
        {
            ViewModel.SelectedUnitValue = UnitSystem.Imperial;
            ViewModel.MinXValue = selected.MinX;
            ViewModel.MaxXValue = selected.MaxX;
            ViewModel.MinYValue = selected.MinY;
            ViewModel.MaxYValue = selected.MaxY;
            ViewModel.MinZValue = selected.MinZ;
            ViewModel.MaxZValue = selected.MaxZ;

            if (!string.IsNullOrWhiteSpace(selected.Volume) &&
                double.TryParse(selected.Volume.Replace(" in³", "").Trim(),
                                System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out double volumeIn3))
            {
                ViewModel.TotalCubicVolume = volumeIn3;
                ViewModel.TotalCubicVolumeScale = VolumeConverter.ConvertIn3ToFt3(volumeIn3);
            }
            else
            {
                ViewModel.TotalCubicVolume = 0;
                ViewModel.TotalCubicVolumeScale = 0;
            }

            ViewModel.SelectedBackgroundValue = selected.Theme?.Contains("Light") == true
                ? BackgroundType.Light
                : BackgroundType.Dark;
        }
    }

    private void DataGridMetric_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem is Axis.Scad.Models.GeneratedModule selected)
        {
            ViewModel.SelectedUnitValue = UnitSystem.Metric;
            ViewModel.MinXValue = selected.MinX;
            ViewModel.MaxXValue = selected.MaxX;
            ViewModel.MinYValue = selected.MinY;
            ViewModel.MaxYValue = selected.MaxY;
            ViewModel.MinZValue = selected.MinZ;
            ViewModel.MaxZValue = selected.MaxZ;

            if (!string.IsNullOrWhiteSpace(selected.Volume) &&
                 double.TryParse(selected.Volume.Replace(" cm³", "").Trim(),
                                 System.Globalization.NumberStyles.Float,
                                 System.Globalization.CultureInfo.InvariantCulture,
                                 out double volumeCm3))
            {
                ViewModel.TotalCubicVolume = volumeCm3;
                ViewModel.TotalCubicVolumeScale = VolumeConverter.ConvertCm3ToM3(volumeCm3);
            }
            else
            {
                ViewModel.TotalCubicVolume = 0;
                ViewModel.TotalCubicVolumeScale = 0;
            }

            ViewModel.SelectedBackgroundValue = selected.Theme?.Contains("Light") == true
                ? BackgroundType.Light
                : BackgroundType.Dark;
        }
    }

    private bool ShouldExcludePropertyWithAllZeros(string propertyName)
    {
        if (ViewModel?.AxesList == null || !ViewModel.AxesList.Any())
            return false;

        return propertyName switch
        {
            "RangeX" => ViewModel.AxesList.All(s => Math.Abs(s.RangeX) == 0.0),
            "RangeY" => ViewModel.AxesList.All(s => Math.Abs(s.RangeY) == 0.0),
            "RangeZ" => ViewModel.AxesList.All(s => Math.Abs(s.RangeZ) == 0.0),
            "MinX" => ViewModel.AxesList.All(s => Math.Abs(s.MinX) == 0.0),
            "MinY" => ViewModel.AxesList.All(s => Math.Abs(s.MinY) == 0.0),
            "MinZ" => ViewModel.AxesList.All(s => Math.Abs(s.MinZ) == 0.0),
            "MaxX" => ViewModel.AxesList.All(s => Math.Abs(s.MaxX) == 0.0),
            "MaxY" => ViewModel.AxesList.All(s => Math.Abs(s.MaxY) == 0.0),
            "MaxZ" => ViewModel.AxesList.All(s => Math.Abs(s.MaxZ) == 0.0),
            _ => false
        };
    }

    private void AdjustLayoutForNarrowScreen()
    {
        var metricAxisGrid = this.FindControl<ScrollViewer>("MetricAxisGrid");
        if (metricAxisGrid != null)
        {
            Grid.SetRow(metricAxisGrid, 2);
            Grid.SetColumn(metricAxisGrid, 0);
            Grid.SetRowSpan(metricAxisGrid, 1);
            Grid.SetColumnSpan(metricAxisGrid, 2);
        }

        var imperialAxisGrid = this.FindControl<ScrollViewer>("ImperialAxisGrid");
        if (imperialAxisGrid != null)
        {
            Grid.SetRow(imperialAxisGrid, 3);
            Grid.SetColumn(imperialAxisGrid, 0);
            Grid.SetRowSpan(imperialAxisGrid, 1);
            Grid.SetColumnSpan(imperialAxisGrid, 2);
        }
    }

    private void AdjustLayoutForWideScreen()
    {
        var metricAxisGrid = this.FindControl<ScrollViewer>("MetricAxisGrid");
        if (metricAxisGrid != null)
        {
            Grid.SetRow(metricAxisGrid, 0);
            Grid.SetColumn(metricAxisGrid, 1);
            Grid.SetRowSpan(metricAxisGrid, 1);
            Grid.SetColumnSpan(metricAxisGrid, 2);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _clientSizeObserver?.Dispose();
        _clientSizeObserver = null;
        _parentWindow = null;
    }
}