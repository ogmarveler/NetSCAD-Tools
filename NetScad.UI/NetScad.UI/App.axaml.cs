using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetScad.UI.ViewModels;
using NetScad.UI.Views;
using System.Diagnostics.CodeAnalysis;

namespace NetScad.UI
{
    public partial class App : Application
    {
        // Static Host property to access DI container
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public override async void OnFrameworkInitializationCompleted()
        {
            // If you use CommunityToolkit, line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Host?.Services.GetRequiredService<MainWindowViewModel>()
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainWindow
                {
                    DataContext = Host?.Services.GetRequiredService<MainWindowViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
        public static IHost? Host { get; set; }
    }
}