using System.Text;

namespace NetScad.Core.Utility
{
    /// <summary>
    /// Utility for wrapping OpenSCAD code into modules
    /// </summary>
    public static class WrapInModule
    {
        /// <summary>
        /// Wraps an OpenSCAD method string into a module with a sanitized name
        /// </summary>
        /// <param name="OSCADMethod">The OpenSCAD code to wrap</param>
        /// <param name="name">Module name (whitespace will be replaced with underscores)</param>
        /// <param name="description">Module description (whitespace will be replaced with underscores)</param>
        /// <returns>A complete OpenSCAD module definition</returns>
        public static string ToModule(string OSCADMethod, string name, string description = "")
        {
            // Sanitize and trim the input OSCADMethod
            var sanitizedMethod = OSCADMethod
                ?.Trim()
                .TrimStart('"')
                .TrimEnd('"')
                .Replace("\\\"", "\"")
                .Trim() ?? string.Empty;

            // Sanitize name and description - replace whitespace with underscores
            var sanitizedName = name
                ?.Trim()
                .Replace(" ", "_")
                .Replace("\t", "_")
                .Replace("\n", "_")
                .Replace("\r", "_") ?? "unnamed_module";

            var sanitizedDescription = description
                ?.Trim()
                .Replace(" ", "_")
                .Replace("\t", "_")
                .Replace("\n", "_")
                .Replace("\r", "_") ?? string.Empty;

            // Build module name
            var moduleName = string.IsNullOrWhiteSpace(sanitizedDescription)
                ? sanitizedName
                : $"{sanitizedName}_{sanitizedDescription}";

            // Return the module definition
            return $"module {moduleName}() {{ {sanitizedMethod} }}";
        }

        /// <summary>
        /// Wraps an OpenSCAD method string into a module with parameters
        /// </summary>
        /// <param name="OSCADMethod">The OpenSCAD code to wrap</param>
        /// <param name="name">Module name (whitespace will be replaced with underscores)</param>
        /// <param name="description">Module description (whitespace will be replaced with underscores)</param>
        /// <param name="parameters">Module parameters (e.g., "width = 10, height = 20")</param>
        /// <returns>A complete OpenSCAD module definition with parameters</returns>
        public static string ToModuleWithParams(string OSCADMethod, string name, string description = "", string parameters = "")
        {
            // Sanitize and trim the input OSCADMethod
            var sanitizedMethod = OSCADMethod
                ?.Trim()
                .TrimStart('"')
                .TrimEnd('"')
                .Replace("\\\"", "\"")
                .Trim() ?? string.Empty;

            // Sanitize name and description - replace whitespace with underscores
            var sanitizedName = name
                ?.Trim()
                .Replace(" ", "_")
                .Replace("\t", "_")
                .Replace("\n", "_")
                .Replace("\r", "_") ?? "unnamed_module";

            var sanitizedDescription = description
                ?.Trim()
                .Replace(" ", "_")
                .Replace("\t", "_")
                .Replace("\n", "_")
                .Replace("\r", "_") ?? string.Empty;

            var sanitizedParams = parameters?.Trim() ?? string.Empty;

            // Build module name
            var moduleName = string.IsNullOrWhiteSpace(sanitizedDescription)
                ? sanitizedName
                : $"{sanitizedName}_{sanitizedDescription}";

            // Return the module definition with parameters
            return $"module {moduleName}({sanitizedParams}) {{ {sanitizedMethod} }}";
        }

        /// <summary>
        /// Creates a union module that combines multiple OpenSCAD objects
        /// </summary>
        /// <param name="osCADMethods">List of OpenSCAD method strings to union</param>
        /// <param name="name">Module name (whitespace will be replaced with underscores)</param>
        /// <param name="solidType">Module solid type (whitespace will be replaced with underscores)</param>
        /// <returns>A module definition with a union operation</returns>
        public static string ToUnionModule(List<string> osCADMethods, string name, string solidType)
        {
            // Sanitize name
            var sanitizedName = name
                ?.Trim()
                .Replace(" ", "_")
                .Replace("\t", "_")
                .Replace("\n", "_")
                .Replace("\r", "_") ?? "unnamed_union";

            // Sanitize solidType
            var sanitizedSolidType = solidType
                ?.Trim()
                .Replace(" ", "_")
                .Replace("\t", "_")
                .Replace("\n", "_")
                .Replace("\r", "_") ?? "nonsolid_difference";

            // Sanitize and combine all OSCAD methods
            var sanitizedMethods = osCADMethods
                ?.Select(method => method
                    ?.Trim()
                    .TrimStart('"')
                    .TrimEnd('"')
                    .Replace("\\\"", "\"")
                    .Trim() ?? string.Empty)
                .Where(method => !string.IsNullOrWhiteSpace(method))
                .ToList() ?? new List<string>();

            // Join all methods with spaces
            var combinedMethods = string.Join(" ", sanitizedMethods);

            // Return the union module
            return $"module union_{sanitizedName}_{sanitizedSolidType}() {{ union() {{ {combinedMethods} }} }}";
        }

        /// <summary>
        /// Creates a difference module that subtracts one object from another
        /// </summary>
        /// <param name="baseObject">The base OpenSCAD object (to subtract from)</param>
        /// <param name="subtractObject">The object to subtract from the base</param>
        /// <param name="name">Module name (whitespace will be replaced with underscores)</param>
        /// <param name="solidType">Module solid type (whitespace will be replaced with underscores)</param>
        /// <returns>A module definition with a difference operation</returns>
        public static string ToDifferenceModule(string baseObject, List<string> subtractObjects, string name, string solidType)
        {
            // Sanitize name
            var sanitizedName = name
                ?.Trim()
                .Replace(" ", "_")
                .Replace("\t", "_")
                .Replace("\n", "_")
                .Replace("\r", "_") ?? "unnamed_difference";

            // Sanitize solidType
            var sanitizedSolidType = solidType
                ?.Trim()
                .Replace(" ", "_")
                .Replace("\t", "_")
                .Replace("\n", "_")
                .Replace("\r", "_") ?? "nonsolid_difference";

            // Sanitize base object
            var sanitizedBase = baseObject
                ?.Trim()
                .TrimStart('"')
                .TrimEnd('"')
                .Replace("\\\"", "\"")
                .Trim() ?? string.Empty;

            var sb = new StringBuilder();
            foreach (var obj in subtractObjects)
            {
                // Sanitize subtract object
                var sanitizedSubtract = obj
                ?.Trim()
                .TrimStart('"')
                .TrimEnd('"')
                .Replace("\\\"", "\"")
                .Trim() ?? string.Empty;

                sb.Append($" {sanitizedSubtract}");
            }

            // Return the difference module
            return $"module difference_{sanitizedName}_{sanitizedSolidType}() {{ difference() {{ {sanitizedBase} {sb.ToString()} }} }}";
        }

        /// <summary>
        /// Creates a module call string (for invoking the module)
        /// </summary>
        /// <param name="name">Module name</param>
        /// <param name="description">Module description</param>
        /// <param name="arguments">Optional arguments to pass to the module</param>
        /// <returns>A module call string</returns>
        public static string CallModule(string name, string description = "", string arguments = "")
        {
            var sanitizedName = name
                ?.Trim()
                .Replace(" ", "_")
                .Replace("\t", "_")
                .Replace("\n", "_")
                .Replace("\r", "_") ?? "unnamed_module";

            var sanitizedDescription = description
                ?.Trim()
                .Replace(" ", "_")
                .Replace("\t", "_")
                .Replace("\n", "_")
                .Replace("\r", "_") ?? string.Empty;

            var sanitizedArgs = arguments?.Trim() ?? string.Empty;

            var moduleName = string.IsNullOrWhiteSpace(sanitizedDescription)
                ? sanitizedName
                : $"{sanitizedName}_{sanitizedDescription}";

            return $"{moduleName}({sanitizedArgs});";
        }
    }
}
