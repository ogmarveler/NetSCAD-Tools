using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace NetGenCAD.Core.Utility
{
    // Enum to specify which project in the solution
    public enum ProjectType
    {
        NetSCAD_Core,  // For NetGenCAD.Core project
        NetSCAD_Axis,  // For NetGenCAD.Axis project
        NetSCAD_UI   // For NetGenCAD.UI project
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
                ProjectType.NetSCAD_Core => "NetGenCAD.Core",
                ProjectType.NetSCAD_Axis => "NetGenCAD.Axis",
                ProjectType.NetSCAD_UI => "NetGenCAD.UI",
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
