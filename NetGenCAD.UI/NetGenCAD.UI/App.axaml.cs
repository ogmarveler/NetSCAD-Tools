using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NetGenCAD.UI.ViewModels;
using NetGenCAD.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace NetGenCAD.UI
{
    public partial class App : Application
    {
        // Static Host property to access DI container
        public override void Initialize() => AvaloniaXamlLoader.Load(this);
        public static IServiceProvider? Services { get; set; }
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Services!.GetRequiredService<MainWindowViewModel>()
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainWindow
                {
                    DataContext = Services!.GetRequiredService<MainWindowViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
        //public static IHost? Host { get; set; }
    }
}