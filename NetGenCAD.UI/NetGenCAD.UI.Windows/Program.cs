using Avalonia;
using Avalonia.Markup.Xaml;
using NetGenCAD.Core.Interfaces;
using NetGenCAD.Core.Models;
using NetGenCAD.UI;
using NetGenCAD.UI.ViewModels;
using NetGenCAD.UI.Views;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Avalonia;
using ReactiveUI.Avalonia.Splat; // Autofac, DryIoc, Ninject, Microsoft.Extensions.DependencyInjection integrations
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NetGenCAD
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    [RequiresUnreferencedCode("MainWindowViewModel uses reflection or dynamic code that may be trimmed.")]
    internal sealed class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var rid = GetRuntimeIdentifier(); // e.g., "win-x64"

            BuildAvaloniaApp(rid).StartWithClassicDesktopLifetime(args, shutdownMode: Avalonia.Controls.ShutdownMode.OnMainWindowClose);
        }

        [RequiresUnreferencedCode("MainWindowViewModel uses reflection or dynamic code that may be trimmed.")]
        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp(string rid)
            => AppBuilder.Configure<App>()
            .UseSkia() // Skia rendering
            .UsePlatformDetect() // Auto-selects platform
            .With(new SkiaOptions()) // Limit GPU memory usage
            .With(new Win32PlatformOptions { RenderingMode = [Win32RenderingMode.AngleEgl, Win32RenderingMode.Wgl, Win32RenderingMode.Software] }) // Enable GPU on Windows
            .WithInterFont() // Use Inter font by default
            .UseReactiveUIWithMicrosoftDependencyResolver(
            services =>
            {
                //services.AddLogging(); // Add logging support

                // Register dbPath as a singleton
                services.AddSingleton(provider =>
                {
                    //var logger = provider.GetRequiredService<ILogger<Program>>();
                    var dbPath = GetDbPath();
                    //logger.LogInformation("Using DB path: {DbPath}, RID: {Rid}", dbPath, rid);
                    return dbPath;
                });

                // Register in Program.cs
                services.AddSingleton<IScadPathProvider>(provider =>
                {
                    //var logger = provider.GetRequiredService<ILogger<Program>>();
                    var scadPath = GetScadPath(); // Your method to get the path
                    //logger.LogInformation("Using SCAD path: {ScadPath}, RID: {Rid}", scadPath, rid);
                    return new ScadPathProvider(scadPath);
                });

                // Singleton SqliteConnection (opened here)
                services.AddSingleton(provider =>
                {
                    var dbPath = provider.GetRequiredService<string>();
                    SqliteConnection.ClearAllPools();
                    var connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadWrite;Cache=Shared");
                    connection.Open(); // Open connection here    
                    return connection;
                });
                services.AddSingleton<MainWindowViewModel>(); // Root VM for MainWindow.axaml embedding
                services.AddSingleton<CreateAxesViewModel>();
                services.AddSingleton<CreateAxesView>();
                services.AddSingleton<AxisView>();
                services.AddSingleton<AxisViewModel>();
                services.AddSingleton<DesignerView>();
                services.AddSingleton<DesignerViewModel>();
                services.AddSingleton<ScadObjectView>();
                services.AddSingleton<ScadObjectViewModel>();
                services.AddSingleton<IScrewSizeService, ScrewSizeService>();
                services.AddSingleton<App>(); // Avalonia app
            },
            withResolver: sp =>
            {
                App.Services = sp; // Set static Services property for DI access throughout app
            },
            withReactiveUIBuilder: rxui =>
            {
                // Optional ReactiveUI customizations
            })
            .RegisterReactiveUIViewsFromEntryAssembly(); // MVVM framework

        private static string GetRuntimeIdentifier()
        {
            if (OperatingSystem.IsWindows()) return "win-x64"; // Or detect ARM if needed
            throw new PlatformNotSupportedException("Unsupported platform");
        }

        private static string GetScadPath()
        {
            // Use bin directory for object.scad
            return Path.Combine(AppContext.BaseDirectory, "Scad");
        }

        private static string GetDbPath()
        {
            // Use bin directory for netscad.db
            return Path.Combine(AppContext.BaseDirectory, "Data", "netscad.db");
        }
    }
}
