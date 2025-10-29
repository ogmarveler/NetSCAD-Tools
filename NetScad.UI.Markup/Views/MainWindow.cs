using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using NetScad.UI.ViewModels;
using System;
using System.Reflection.Metadata;
using System.Resources;  

namespace NetScad.UI.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            // Window properties
            Title = "NetSCAD";
            Width = 1280;
            Height = 720;
            Icon = new WindowIcon("/Assets/Images/logo.png");

            // Bind to dynamic resource for background
            this.Bind(BackgroundProperty, new DynamicResourceExtension("WindowBackground"));

            // Load resources
            var resources = new ResourceDictionary();
            resources.MergedDictionaries.Add(
                (ResourceDictionary)AvaloniaXamlLoader.Load(
                    new Uri("avares://NetScad.UI/Resources/CardResources.axaml")
                )
            );
            Resources = resources;

            // Build the UI hierarchy
            Content = BuildContent();
        }

        private Control BuildContent()
        {
            // Create the DockPanel (root container)
            var dockPanel = new DockPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Create the Menu
            var menu = BuildMenu();
            DockPanel.SetDock(menu, Dock.Top);
            dockPanel.Children.Add(menu);

            // Create the ScrollViewer with ContentControl
            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var contentControl = new ContentControl
            {
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            };

            // Bind to MainViewContent property in ViewModel
            contentControl.Bind(
                ContentControl.ContentProperty,
                new Binding("MainViewContent")
            );

            scrollViewer.Content = contentControl;
            dockPanel.Children.Add(scrollViewer);

            return dockPanel;
        }

        private Menu BuildMenu()
        {
            var menu = new Menu();

            // "_Open" MenuItem
            var openMenuItem = new MenuItem { Header = "_Open" };
            var openFolderItem = new MenuItem { Header = "_SCAD Folder" };
            openFolderItem.Bind(MenuItem.CommandProperty, new Binding("OpenFolderCommand"));
            openFolderItem.Icon = new PathIcon
            {
                Data = (Geometry)Application.Current!.FindResource("folder_open_regular")!
            };
            openMenuItem.Items.Add(openFolderItem);

            // "_Create" MenuItem
            var createMenuItem = new MenuItem { Header = "_Create" };

            var newAxesItem = new MenuItem { Header = "_New Axes" };
            newAxesItem.Bind(MenuItem.CommandProperty, new Binding("NewAxesCommand"));
            newAxesItem.Icon = new PathIcon
            {
                Data = (Geometry)Application.Current!.FindResource("arrow_expand_regular")!
            };

            var newObjectItem = new MenuItem { Header = "_New Object" };
            newObjectItem.Bind(MenuItem.CommandProperty, new Binding("NewObjectCommand"));
            newObjectItem.Icon = new PathIcon
            {
                Data = (Geometry)Application.Current!.FindResource("select_object_regular")!
            };

            createMenuItem.Items.Add(newAxesItem);
            createMenuItem.Items.Add(newObjectItem);

            // "_Appearance" MenuItem
            var appearanceMenuItem = new MenuItem { Header = "_Appearance" };
            var toggleThemeItem = new MenuItem { Header = "_Switch Theme" };
            toggleThemeItem.Bind(MenuItem.CommandProperty, new Binding("ToggleCommand"));
            toggleThemeItem.Icon = new PathIcon
            {
                Data = (Geometry)Application.Current!.FindResource("dark_theme_regular")!
            };
            appearanceMenuItem.Items.Add(toggleThemeItem);

            // "_Guides" MenuItem
            var guidesMenuItem = new MenuItem { Header = "_Guides" };

            var axisViewItem = new MenuItem { Header = "_Custom Axis" };
            axisViewItem.Bind(MenuItem.CommandProperty, new Binding("AxisViewCommand"));
            axisViewItem.Icon = new PathIcon
            {
                Data = (Geometry)Application.Current!.FindResource("book_question_mark_regular")!
            };

            var designerViewItem = new MenuItem { Header = "_Object Designer" };
            designerViewItem.Bind(MenuItem.CommandProperty, new Binding("DesignerViewCommand"));
            designerViewItem.Icon = new PathIcon
            {
                Data = (Geometry)Application.Current!.FindResource("book_question_mark_regular")!
            };

            guidesMenuItem.Items.Add(axisViewItem);
            guidesMenuItem.Items.Add(designerViewItem);

            // Add all top-level menu items
            menu.Items.Add(openMenuItem);
            menu.Items.Add(createMenuItem);
            menu.Items.Add(appearanceMenuItem);
            menu.Items.Add(guidesMenuItem);

            return menu;
        }
    }
}