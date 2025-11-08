using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NetScad.Designer.Utility
{
    /// <summary>
    /// Utility class for handling SCAD file operations including opening files and showing in file explorer
    /// </summary>
    public static class ScadFileOperations
    {
        // Track opened files to prevent duplicates
        private static readonly HashSet<string> _openedFiles = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new();

        /// <summary>
        /// Opens a .scad file using the default application associated with .scad files.
        /// This respects the user's preference for OpenSCAD or any other SCAD IDE.
        /// </summary>
        /// <param name="scadFilePath">Full path to the .scad file</param>
        /// <param name="allowDuplicates">If false, prevents opening the same file multiple times</param>
        /// <returns>True if successful, false otherwise</returns>
        /// <exception cref="FileNotFoundException">Thrown when the SCAD file does not exist</exception>
        public static async Task<bool> OpenScadFileAsync(string scadFilePath, bool allowDuplicates = false)
        {
            if (string.IsNullOrWhiteSpace(scadFilePath))
            {
                throw new ArgumentException("SCAD file path cannot be null or empty", nameof(scadFilePath));
            }

            if (!File.Exists(scadFilePath))
            {
                throw new FileNotFoundException($"SCAD file not found: {scadFilePath}");
            }

            // Normalize the path for comparison
            var normalizedPath = Path.GetFullPath(scadFilePath);

            bool alreadyOpen = false;
            // Check if file is already tracked as open
            if (!allowDuplicates)
            {
                lock (_lock)
                {
                    if (_openedFiles.Contains(normalizedPath))
                    {
                        alreadyOpen = true;
                    }
                }
                if (alreadyOpen)
                {
                    Console.WriteLine($"File already open: {normalizedPath}");

                    // Try to bring the existing window to front
                    await BringToFrontAsync(normalizedPath);
                    return true;
                }
            }

            try
            {
                // Let the OS decide which application to use based on file association
                var startInfo = new ProcessStartInfo
                {
                    FileName = scadFilePath,
                    UseShellExecute = true // Critical: This tells OS to use default app
                };

                Process.Start(startInfo);

                // Track the opened file
                if (!allowDuplicates)
                {
                    lock (_lock)
                    {
                        _openedFiles.Add(normalizedPath);
                    }
                }

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening SCAD file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a file is currently tracked as open
        /// </summary>
        /// <param name="scadFilePath">Full path to the .scad file</param>
        /// <returns>True if the file is tracked as open</returns>
        public static bool IsFileTrackedAsOpen(string scadFilePath)
        {
            if (string.IsNullOrWhiteSpace(scadFilePath))
            {
                return false;
            }

            var normalizedPath = Path.GetFullPath(scadFilePath);
            
            lock (_lock)
            {
                return _openedFiles.Contains(normalizedPath);
            }
        }

        /// <summary>
        /// Checks if a file is actually open by checking running processes (Windows only)
        /// </summary>
        /// <param name="scadFilePath">Full path to the .scad file</param>
        /// <returns>True if the file appears to be open in a process</returns>
        public static bool IsFileOpenInProcess(string scadFilePath)
        {
            if (string.IsNullOrWhiteSpace(scadFilePath) || !File.Exists(scadFilePath))
            {
                return false;
            }

            try
            {
                // Try to open the file exclusively - if it's locked, it's likely open
                using var stream = File.Open(scadFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Close();
                return false; // File is not locked
            }
            catch (IOException)
            {
                // File is locked, likely open in another application
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to bring an already-open file window to the front (platform-specific)
        /// </summary>
        /// <param name="scadFilePath">Full path to the .scad file</param>
        private static async Task BringToFrontAsync(string scadFilePath)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // On Windows, try to activate the window containing the file
                    // This is a best-effort approach - may not always work
                    var fileName = Path.GetFileName(scadFilePath);
                    
                    // PowerShell script to find and activate window
                    var psScript = $@"
                        $windows = Get-Process | Where-Object {{$_.MainWindowTitle -like '*{fileName}*'}}
                        if ($windows) {{
                            Add-Type @""
                                using System;
                                using System.Runtime.InteropServices;
                                public class Win32 {{
                                    [DllImport(""user32.dll"")] 
                                    public static extern bool SetForegroundWindow(IntPtr hWnd);
                                }}
""@
                            [Win32]::SetForegroundWindow($windows[0].MainWindowHandle)
                        }}
                    ";

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    Process.Start(startInfo);
                }
                // Linux and macOS don't have reliable cross-application window focusing
            }
            catch
            {
                // Best-effort, ignore failures
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Removes a file from the tracking list (call this when you know a file has been closed)
        /// </summary>
        /// <param name="scadFilePath">Full path to the .scad file</param>
        public static void UntrackFile(string scadFilePath)
        {
            if (string.IsNullOrWhiteSpace(scadFilePath))
            {
                return;
            }

            var normalizedPath = Path.GetFullPath(scadFilePath);
            
            lock (_lock)
            {
                _openedFiles.Remove(normalizedPath);
            }
        }

        /// <summary>
        /// Clears all tracked files
        /// </summary>
        public static void ClearTrackedFiles()
        {
            lock (_lock)
            {
                _openedFiles.Clear();
            }
        }

        /// <summary>
        /// Opens a .scad file using the default application associated with .scad files.
        /// This respects the user's preference for OpenSCAD or any other SCAD IDE.
        /// </summary>
        /// <param name="scadFilePath">Full path to the .scad file</param>
        /// <returns>True if successful, false otherwise</returns>
        /// <exception cref="FileNotFoundException">Thrown when the SCAD file does not exist</exception>
        public static async Task<bool> ShowInExplorerAsync(string scadFilePath)
        {
            if (string.IsNullOrWhiteSpace(scadFilePath))
            {
                throw new ArgumentException("SCAD file path cannot be null or empty", nameof(scadFilePath));
            }

            if (!File.Exists(scadFilePath))
            {
                throw new FileNotFoundException($"SCAD file not found: {scadFilePath}");
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select, \"{scadFilePath}\"",
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Most Linux file managers support opening parent directory
                    var directory = Path.GetDirectoryName(scadFilePath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "xdg-open",
                            Arguments = $"\"{directory}\"",
                            UseShellExecute = true
                        });
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "open",
                        Arguments = $"-R \"{scadFilePath}\"", // -R reveals file in Finder
                        UseShellExecute = true
                    });
                }

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing file in explorer: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opens a folder in the system's file explorer
        /// </summary>
        /// <param name="folderPath">Full path to the folder</param>
        /// <returns>True if successful, false otherwise</returns>
        /// <exception cref="DirectoryNotFoundException">Thrown when the folder does not exist</exception>
        public static async Task<bool> OpenFolderAsync(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("Folder path cannot be null or empty", nameof(folderPath));
            }

            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"Folder not found: {folderPath}");
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{folderPath}\"",
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "xdg-open",
                        Arguments = $"\"{folderPath}\"",
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "open",
                        Arguments = $"\"{folderPath}\"",
                        UseShellExecute = true
                    });
                }

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening folder: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a SCAD file exists at the specified path
        /// </summary>
        /// <param name="scadFilePath">Full path to the .scad file</param>
        /// <returns>True if the file exists, false otherwise</returns>
        public static bool ScadFileExists(string scadFilePath)
        {
            if (string.IsNullOrWhiteSpace(scadFilePath))
            {
                return false;
            }

            return File.Exists(scadFilePath) && 
                   Path.GetExtension(scadFilePath).Equals(".scad", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the full path for a SCAD file in the Solids directory
        /// </summary>
        /// <param name="scadPath">Base SCAD path from IScadPathProvider</param>
        /// <param name="fileName">Name of the SCAD file</param>
        /// <returns>Full path to the SCAD file</returns>
        public static string GetSolidsFilePath(string scadPath, string fileName)
        {
            if (string.IsNullOrWhiteSpace(scadPath))
            {
                throw new ArgumentException("SCAD path cannot be null or empty", nameof(scadPath));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            }

            // Ensure the file has .scad extension
            if (!fileName.EndsWith(".scad", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".scad";
            }

            return Path.Combine(scadPath, "Solids", fileName);
        }

        /// <summary>
        /// Ensures the Solids directory exists, creating it if necessary
        /// </summary>
        /// <param name="scadPath">Base SCAD path from IScadPathProvider</param>
        /// <returns>Full path to the Solids directory</returns>
        public static string EnsureSolidsDirectoryExists(string scadPath)
        {
            if (string.IsNullOrWhiteSpace(scadPath))
            {
                throw new ArgumentException("SCAD path cannot be null or empty", nameof(scadPath));
            }

            var solidsPath = Path.Combine(scadPath, "Solids");
            
            if (!Directory.Exists(solidsPath))
            {
                Directory.CreateDirectory(solidsPath);
            }

            return solidsPath;
        }

        /// <summary>
        /// Exports a .scad file to STL format using OpenSCAD
        /// </summary>
        /// <param name="scadFilePath">Full path to the .scad file</param>
        /// <param name="stlFilePath">Full path for the output .stl file (optional, defaults to same name)</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> ExportToStlAsync(string scadFilePath, string? stlFilePath = null)
        {
            if (string.IsNullOrWhiteSpace(scadFilePath))
            {
                throw new ArgumentException("SCAD file path cannot be null or empty", nameof(scadFilePath));
            }

            if (!File.Exists(scadFilePath))
            {
                throw new FileNotFoundException($"SCAD file not found: {scadFilePath}");
            }

            // Default STL path to same directory with .stl extension
            stlFilePath ??= Path.ChangeExtension(scadFilePath, ".stl");

            try
            {
                Console.WriteLine($"Exporting STL: {scadFilePath} -> {stlFilePath}");

                string openScadPath = FindOpenScadExecutable();
                
                if (string.IsNullOrEmpty(openScadPath))
                {
                    Console.WriteLine("OpenSCAD executable not found. Please ensure OpenSCAD is installed.");
                    return false;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = openScadPath,
                    Arguments = $"-o \"{stlFilePath}\" \"{scadFilePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    Console.WriteLine("Failed to start OpenSCAD process");
                    return false;
                }

                await process.WaitForExitAsync();

                if (process.ExitCode == 0 && File.Exists(stlFilePath))
                {
                    var fileInfo = new FileInfo(stlFilePath);
                    Console.WriteLine($"STL exported successfully: {stlFilePath} ({fileInfo.Length:N0} bytes)");
                    return true;
                }
                else
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    Console.WriteLine($"STL export failed: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting to STL: {ex.Message}");
                return false;
            }
        }

        private static string FindOpenScadExecutable()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var paths = new[]
                {
                    @"C:\Program Files\OpenSCAD\openscad.exe",
                    @"C:\Program Files (x86)\OpenSCAD\openscad.exe"
                };

                return paths.FirstOrDefault(File.Exists) ?? "openscad.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var paths = new[] { "/usr/bin/openscad", "/usr/local/bin/openscad" };
                return paths.FirstOrDefault(File.Exists) ?? "openscad";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "/Applications/OpenSCAD.app/Contents/MacOS/OpenSCAD";
            }

            return "openscad";
        }
    }
}
