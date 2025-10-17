using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NetScad.Designer.Utility
{
    /// <summary>
    /// Utility class for handling SCAD file operations including opening files and showing in file explorer
    /// </summary>
    public static class ScadFileOperations
    {
        /// <summary>
        /// Opens a .scad file using the default application associated with .scad files.
        /// This respects the user's preference for OpenSCAD or any other SCAD IDE.
        /// </summary>
        /// <param name="scadFilePath">Full path to the .scad file</param>
        /// <returns>True if successful, false otherwise</returns>
        /// <exception cref="FileNotFoundException">Thrown when the SCAD file does not exist</exception>
        public static async Task<bool> OpenScadFileAsync(string scadFilePath)
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
                // Let the OS decide which application to use based on file association
                var startInfo = new ProcessStartInfo
                {
                    FileName = scadFilePath,
                    UseShellExecute = true // Critical: This tells OS to use default app
                };

                Process.Start(startInfo);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening SCAD file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opens the containing folder and selects/highlights the .scad file
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
    }
}
