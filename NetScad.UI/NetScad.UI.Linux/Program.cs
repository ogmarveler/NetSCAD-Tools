using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetScad.Core.Interfaces;
using NetScad.Core.Models;
using NetScad.UI;
using NetScad.UI.ViewModels;
using NetScad.UI.Views;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace NetScad
{
    internal sealed class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);
            var rid = GetRuntimeIdentifier(); // e.g., "win-x64", "linux-x64", "linux-arm64"
            builder.ConfigureServices((context, services) =>
            {
                // Register dbPath as a singleton
                services.AddSingleton(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<Program>>();
                    var dbPath = GetDbPath();
                    logger.LogInformation("Using DB path: {DbPath}, RID: {Rid}", dbPath, rid);
                    return dbPath;
                });

                // Register in Program.cs
                services.AddSingleton<IScadPathProvider>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<Program>>();
                    var scadPath = GetScadPath(); // Your method to get the path
                    logger.LogInformation("Using SCAD path: {ScadPath}, RID: {Rid}", scadPath, rid);
                    return new ScadPathProvider(scadPath);
                });

                // Singleton SqliteConnection (opened here)
                services.AddSingleton(provider =>
                {
                    var dbPath = provider.GetRequiredService<string>();
                    // Clear any open pools
                    SqliteConnection.ClearAllPools();
                    GC.Collect();
                    var connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadWrite;Cache=Shared");
                    connection.Open(); // Open connection here
                    return connection;
                });

                services.AddSingleton<MainWindowViewModel>(); // Root VM for MainWindow.axaml embedding
                services.AddSingleton<CreateAxesViewModel>();
                services.AddSingleton<CreateAxesView>();
                services.AddSingleton<AxisView>();
                services.AddSingleton<AxisViewModel>();
                services.AddSingleton<ScadObjectView>();
                services.AddSingleton<ScadObjectViewModel>();
                services.AddSingleton<IScrewSizeService, ScrewSizeService>();
                services.AddSingleton<App>(); // Avalonia app
            });

            // Build and start the host
            var host = builder.Build();
            App.Host = host; // Set static Host property for DI access throughout app
            host.StartAsync();

            try
            {
                if (args.Contains("--drm")) { SilenceConsole(); BuildAvaloniaApp().StartLinuxDrm(args, "/dev/dri/card1", 1D); }
                else { BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, shutdownMode: Avalonia.Controls.ShutdownMode.OnMainWindowClose); }
            }
            finally
            {
                // Graceful shutdown: Dispose connection
                host.StopAsync(TimeSpan.FromSeconds(5));
                host.Dispose();
                GC.Collect();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
            .UseSkia() // Skia rendering
            .UsePlatformDetect() // Auto-selects X11, Wayland, etc., on Linux
            .With(new SkiaOptions()) // Limit GPU memory usage
            .With(new Win32PlatformOptions { RenderingMode = [Win32RenderingMode.AngleEgl, Win32RenderingMode.Wgl, Win32RenderingMode.Software] }) // Enable GPU on Windows
            .With(new MacOSPlatformOptions { ShowInDock = true, }) // Options on macOS
            .With(new X11PlatformOptions { RenderingMode = [X11RenderingMode.Glx, X11RenderingMode.Software], OverlayPopups = true, UseDBusMenu = true, WmClass = AppDomain.CurrentDomain.FriendlyName, }) // Enable GPU on Linux
            .WithInterFont() // Use Inter font by default
            .UseReactiveUI(); // MVVM framework

        private static void SilenceConsole()
        {
            new Thread(() =>
            {
                Console.CursorVisible = false;
                while (true)
                    Console.ReadKey(true);
            })
            { IsBackground = true }.Start();
        }

        private static string GetRuntimeIdentifier()
        {
            if (OperatingSystem.IsWindows()) return "win-x64"; // Or detect ARM if needed
            if (OperatingSystem.IsLinux())
            {
                if (RuntimeInformation.OSArchitecture == Architecture.Arm64) return "linux-arm64";
                return "linux-x64";
            }
            throw new PlatformNotSupportedException("Unsupported platform");
        }

        private static string GetScadPath()
        {
            // Use bin directory for object.scad
            return Path.Combine(AppContext.BaseDirectory, "Scad");
        }

        private static string GetDbPath()
        {
            // Check server-side path first (for sync)
            var serverPath = Environment.GetEnvironmentVariable("NETSCAD_DB_PATH");
            if (!string.IsNullOrEmpty(serverPath) && File.Exists(serverPath))
                return serverPath;

            // Use bin directory for netscad.db
            return Path.Combine(AppContext.BaseDirectory, "Data", "netscad.db");
        }
    }
}
