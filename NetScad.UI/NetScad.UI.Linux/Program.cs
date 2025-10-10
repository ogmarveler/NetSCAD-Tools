using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetScad.UI.ViewModels;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using NetScad.UI;
using NetScad.Core.Services;
using System.IO;

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

                // Singleton SqliteConnection (not opened here)
                services.AddSingleton(provider =>
                {
                    var dbPath = provider.GetRequiredService<string>();
                    var connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadWrite;Cache=Shared");
                    return connection;
                });

                services.AddHostedService<DbInitializationService>(); // Background service for DB initialization (off UI thread)
                services.AddSingleton<MainWindowViewModel>(); // Root VM for MainWindow.axaml embedding
                services.AddSingleton<App>(); // Avalonia app
            });

            var host = builder.Build();
            App.Host = host; // Set static Host property
            host.Start();

            try
            {
                if (args.Contains("--drm")) { SilenceConsole(); BuildAvaloniaApp().StartLinuxDrm(args, "/dev/dri/card1", 1D); }
                else { BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, shutdownMode: Avalonia.Controls.ShutdownMode.OnMainWindowClose); }
            }
            finally
            {
                // Graceful shutdown: Dispose connection
                host.StopAsync(TimeSpan.FromSeconds(5)).Wait();
                host.Dispose();
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
            .LogToTrace() // Enable logging to Visual Studio output window
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
