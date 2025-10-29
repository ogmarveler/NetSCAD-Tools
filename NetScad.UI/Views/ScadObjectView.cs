using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using NetScad.UI.ViewModels;

namespace NetScad.UI.Views;

public class ScadObjectView : UserControl
{
    private readonly ScadObjectViewModel _viewModel;
    
    public ScadObjectView(ScadObjectViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        
        Content = BuildUI();
    }
    
    private Control BuildUI()
    {
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(10)
        };
        
        // Header section
        var header = new TextBlock
        {
            Text = "SCAD Object Generator",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        Grid.SetRow(header, 0);
        Grid.SetColumnSpan(header, 2);
        mainGrid.Children.Add(header);
        
        // SCAD Code TextBox
        var scadCodeBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = new FontFamily("Consolas,Courier New,monospace"),
            IsReadOnly = true,
            Margin = new Thickness(0, 0, 10, 0)
        };
        scadCodeBox.Bind(TextBox.TextProperty, new Avalonia.Data.Binding("ScadCode"));
        Grid.SetRow(scadCodeBox, 1);
        Grid.SetColumn(scadCodeBox, 0);
        mainGrid.Children.Add(scadCodeBox);
        
        // Sidebar with controls
        var sidebar = new StackPanel
        {
            Spacing = 10,
            MinWidth = 200
        };
        
        var generateButton = new Button
        {
            Content = "Generate SCAD",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        generateButton.Bind(Button.CommandProperty, new Avalonia.Data.Binding("GenerateScadCommand"));
        sidebar.Children.Add(generateButton);
        
        var volumeText = new TextBlock();
        volumeText.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding("Volume")
        {
            StringFormat = "Volume: {0:N2} cm³"
        });
        sidebar.Children.Add(volumeText);
        
        Grid.SetRow(sidebar, 1);
        Grid.SetColumn(sidebar, 1);
        mainGrid.Children.Add(sidebar);
        
        return mainGrid;
    }
}