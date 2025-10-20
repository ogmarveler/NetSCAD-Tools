using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace NetScad.Core.Utility
{
    // Enum to specify which project in the solution
    public enum ProjectType
    {
        NetSCAD_Core,  // For NetSCAD.Core project
        NetSCAD_Axis,  // For NetSCAD.Axis project
        NetSCAD_UI   // For NetSCAD.UI project
    }

    public static class PathHelper
    {
        [UnconditionalSuppressMessage("SingleFile", "IL3000:Avoid accessing Assembly file path when publishing as a single file", Justification = "<Pending>")]
        public static string GetProjectFolder(ProjectType project)
        {
            // Get the directory of the executing assembly (e.g., bin/Debug/net8.0)
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var binDirectory = Path.GetDirectoryName(assemblyLocation) ?? throw new DirectoryNotFoundException("Cannot determine assembly directory.");

            // Navigate up to the solution root (assumes bin/Debug is 3 levels down from solution)
            var solutionRoot = Path.GetFullPath(Path.Combine(binDirectory, "..", "..", ".."));

            // Map enum to project folder names
            string projectFolder = project switch
            {
                ProjectType.NetSCAD_Core => "NetSCAD.Core",
                ProjectType.NetSCAD_Axis => "NetSCAD.Axis",
                ProjectType.NetSCAD_UI => "NetSCAD.UI",
                _ => throw new ArgumentException($"Unknown project type: {project}", nameof(project))
            };

            // Combine solution root with project folder
            var projectRoot = Path.Combine(solutionRoot, projectFolder);

            // Verify the directory exists
            if (!Directory.Exists(projectRoot))
            {
                throw new DirectoryNotFoundException($"Project root for {project} not found at: {projectRoot}");
            }

            return projectRoot;
        }

        // Cache if called multiple times
        [UnconditionalSuppressMessage("SingleFile", "IL3000:Avoid accessing Assembly file path when publishing as a single file", Justification = "<Pending>")]
        public static string GetProjectRoot()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(assemblyLocation) ?? string.Empty, "..", "..", ".."));
        }
    }
}
