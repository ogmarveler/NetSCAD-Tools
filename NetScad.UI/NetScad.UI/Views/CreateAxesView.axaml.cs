using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using NetScad.Core.Measurements;
using NetScad.UI.Converters;
using NetScad.UI.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static NetScad.Core.Measurements.Selector;

namespace NetScad.UI.Views;

public partial class CreateAxesView : UserControl, INotifyPropertyChanged
{
    private CreateAxesViewModel ViewModel => (CreateAxesViewModel)DataContext!;
    private Window? _parentWindow;
    private IDisposable? _clientSizeObserver;
    private const double WRAP_THRESHOLD_WIDTH = 1400;

    public CreateAxesView()
    {
        InitializeComponent();
        DataContext = App.Services!.GetRequiredService<CreateAxesViewModel>();
        this.AttachedToVisualTree += CreateAxesView_AttachedToVisualTree;

        // Set fixed DataGrid widths after XAML is initialized
        SetDataGridWidths();
    }

    private void SetDataGridWidths()
    {   
        // Find the DataGrids by name (from XAML)
        var metricDataGrid = this.FindControl<DataGrid>("AxesListMetric");
        var imperialDataGrid = this.FindControl<DataGrid>("AxesListImperial");

        if (metricDataGrid != null)
        {
            metricDataGrid.Width = 960;  // Fixed width
            metricDataGrid.MaxWidth = 960;
            metricDataGrid.MinWidth = 600;
            metricDataGrid.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        }   

        if (imperialDataGrid != null)
        {
            imperialDataGrid.Width = 960;  // Fixed width
            imperialDataGrid.MaxWidth = 960;
            imperialDataGrid.MinWidth = 600;
            imperialDataGrid.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
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
        double dataGridWidth = windowWidth < 1200 ? 600 : 860;

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
            // Create a custom DataGridTextColumn with value formatting
            var axisDisplayColumn = new DataGridTextColumn
            {
                Header = "Axis Display",
                Width = new DataGridLength(200),
                CanUserResize = false,
                IsReadOnly = true,
                Binding = new Binding(nameof(Axis.Scad.Models.GeneratedModule.CallingMethod))
                {
                    Converter = new AxisDisplayConverter() // Use the converter directly
                }
            };

            // Replace the auto-generated column
            e.Column = axisDisplayColumn;
            return;
        }

        // Apply centering to the column header
        if (e.Column is DataGridTextColumn textColumn)
        {
            textColumn.HeaderTemplate = new FuncDataTemplate<object>((value, namescope) =>
            {
                return new TextBlock
                {
                    Text = value?.ToString() ?? "",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    TextAlignment = Avalonia.Media.TextAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                };
            });
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

        // Hide columns with all zeros
        if (ShouldExcludePropertyWithAllZeros(e.PropertyName))
        {
            e.Cancel = true;
        }
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

            // Highlight corresponding axis row based on Calling Method
            if (!string.IsNullOrEmpty(selected.CallingMethod))
            {
                HighlightAxisRow(selected, dataGrid);
            }
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

            // Highlight corresponding axis row based on Calling Method
            if (!string.IsNullOrEmpty(selected.CallingMethod))
            {
                HighlightAxisRow(selected, dataGrid);
            }
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

    // New method to highlight a row in a DataGrid by selecting and scrolling to the item
    private void HighlightAxisRow(Axis.Scad.Models.GeneratedModule module, DataGrid dataGrid)
    {
        // Clear previous selections if needed (optional)
        // dataGrid.SelectedItem = null;

        // Select the item and scroll into view
        dataGrid.SelectedItem = module;
        dataGrid.ScrollIntoView(module, null);
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
        //var imperialAxisGrid = this.FindControl<ScrollViewer>("ImperialAxisGrid");
        //if (imperialAxisGrid != null)
        //{
        //    Grid.SetRow(imperialAxisGrid, 0);
        //    Grid.SetColumn(imperialAxisGrid, 1);
        //    Grid.SetRowSpan(imperialAxisGrid, 2);
        //    Grid.SetColumnSpan(imperialAxisGrid, 1);
        //}

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

    /* FUTURE ENHANCEMENT: XAML-based modal popup
     * 
     * Instead of code-behind Window creation, consider:
     * 1. Flyout in XAML bound to ViewModel
     * 2. ContentDialog with data template
     * 3. Popup control with IsOpen binding
     * 
     * Example XAML approach:
     * <Button.Flyout>
     *   <Flyout>
     *     <StackPanel>
     *       <TextBox Text="{Binding SelectedAxis.DisplayName}" IsReadOnly="True"/>
     *       <TextBox Text="{Binding SelectedAxis.CallingMethod}" IsReadOnly="True"/>
     *       <Button Command="{Binding CopyToClipboardCommand}"/>
     *     </StackPanel>
     *   </Flyout>
     * </Button.Flyout>
     */
}