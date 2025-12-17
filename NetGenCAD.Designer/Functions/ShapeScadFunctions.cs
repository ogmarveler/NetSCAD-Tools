using NetGenCAD.Designer.Repositories;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using static NetGenCAD.Core.Measurements.Conversion;
using static NetGenCAD.Core.Measurements.Selector;
using NetGenCAD.Designer.Utility;

namespace NetGenCAD.Designer.Functions
{
    public static class ShapeScadFunctions
    {
        /// <summary>
        /// Converts input dimensions from metric (mm) to imperial (inches).
        /// </summary>
        /// <param name="pointXMM">Polyhedron point X in millimeters</param>
        /// <param name="pointYMM">Polyhedron point Y in millimeters</param>
        /// <param name="pointZMM">Polyhedron point Z in millimeters</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <returns>Tuple containing converted imperial values in the same order</returns>
        public static (double PointX, double PointY, double PointZ) ConvertInputsToImperial(double pointXMM, double pointYMM, double pointZMM, int decimalPlaces)
        {
            // Convert from metric unit system to imperial (mm to inches)
            var convertedPointX = Math.Round(MillimeterToInches(pointXMM), decimalPlaces);
            var convertedPointY = Math.Round(MillimeterToInches(pointYMM), decimalPlaces);
            var convertedPointZ = Math.Round(MillimeterToInches(pointZMM), decimalPlaces);
            return (convertedPointX, convertedPointY, convertedPointZ);
        }

        /// <summary>
        /// Converts input dimensions from imperial (inches) to metric (mm).
        /// </summary>
        /// <param name="pointXInches">Polyhedron point X in inches</param>
        /// <param name="pointYInches">Polyhedron point Y in inches</param>
        /// <param name="pointZInches">Polyhedron point Z in inches</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <returns>Tuple containing converted metric values in the same order</returns>
        public static (double PointX, double PointY, double PointZ) ConvertInputsToMetric(double pointXInches, double pointYInches, double pointZInches, int decimalPlaces)
        {
            // Convert from imperial unit system to metric (inches to mm)
            var convertedPointX = Math.Round(InchesToMillimeter(pointXInches), decimalPlaces);
            var convertedPointY = Math.Round(InchesToMillimeter(pointYInches), decimalPlaces);
            var convertedPointZ = Math.Round(InchesToMillimeter(pointZInches), decimalPlaces);
            return (convertedPointX, convertedPointY, convertedPointZ);
        }

        public delegate Task CreatePolyhedronAsyncCallbackAsync(int polyhedronId, ObservableCollection<PolyhedronDimensions> updatedPolyhedronDimensions);
        /// <summary>
        /// Creates and persists a polyhedron object with callback notification.
        /// Handles polyhedron-specific properties (points, faces, operation type) and persists to database.
        /// Invokes callback with updated polyhedron collections for ViewModel binding.
        /// </summary>
        /// <param name="name">Name of the polyhedron object</param>
        /// <param name="description">Description of the polyhedron</param>
        /// <param name="polyhedronOperationType">Operation type (Points or Faces)</param>
        /// <param name="pointXMM">X coordinate of point in millimeters</param>
        /// <param name="pointYMM">Y coordinate of point in millimeters</param>
        /// <param name="pointZMM">Z coordinate of point in millimeters</param>
        /// <param name="pointsId">Identifier for the point set</param>
        /// <param name="face">Face definition (indices of points)</param>
        /// <param name="faceId">Identifier for the face set</param>
        /// <param name="convexity">Convexity of the polyhedron</param>
        /// <param name="selectedUnit">Unit system (Metric or Imperial)</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <param name="dbConnection">Database connection for persistence</param>
        /// <param name="generateOscadCallback">Callback to generate OSCAD method string</param>
        /// <param name="onPolyhedronCreated">Callback invoked when creation completes</param>
        /// <returns>The ID of the created polyhedron, or 0 if creation failed</returns>
        public static async Task<int> CreatePolyhedronWithCallbackAsync(
            string name,
            string description,
            string polyhedronOperationType,
            double pointXMM,
            double pointYMM,
            double pointZMM,
            int pointsId,
            string face,
            int faceId,
            int convexity,
            UnitSystem selectedUnit,
            int decimalPlaces,
            SqliteConnection dbConnection,
            Func<PolyhedronDimensions, Task<string>> generateOscadCallback,
            CreatePolyhedronAsyncCallbackAsync onPolyhedronCreated)
        {
            try
            {
                // Convert points from imperial to metric if needed
                var convertedPointX = pointXMM;
                var convertedPointY = pointYMM;
                var convertedPointZ = pointZMM;

                if (selectedUnit == UnitSystem.Imperial)
                {
                    convertedPointX = Math.Round(InchesToMillimeter(pointXMM), decimalPlaces);
                    convertedPointY = Math.Round(InchesToMillimeter(pointYMM), decimalPlaces);
                    convertedPointZ = Math.Round(InchesToMillimeter(pointZMM), decimalPlaces);
                }

                // Create new PolyhedronDimensions instance
                var newPolyhedron = new PolyhedronDimensions
                {
                    Name = name,
                    Description = description,
                    PolyhedronOperationType = polyhedronOperationType,
                    PointX_MM = convertedPointX,
                    PointY_MM = convertedPointY,
                    PointZ_MM = convertedPointZ,
                    PointsId = pointsId,
                    Face = face,
                    FaceId = faceId,
                    Convexity = convexity,
                    CreatedAt = DateTime.UtcNow,
                };

                // Generate OSCAD method via callback
                newPolyhedron.OSCADMethod = await generateOscadCallback(newPolyhedron);

                // Save to database
                await newPolyhedron.UpsertAsync(dbConnection);

                // Retrieve all polyhedron dimensions for the object name
                var allPolyhedrons = await new PolyhedronDimensions().GetByObjectNameAsync(dbConnection, name);

                System.Diagnostics.Debug.WriteLine(
                    $"Polyhedron created successfully: ID={newPolyhedron.Id}, Name={name}, Type={polyhedronOperationType}");

                // Invoke callback with updated collections
                await onPolyhedronCreated(
                    newPolyhedron.Id,
                    new ObservableCollection<PolyhedronDimensions>(allPolyhedrons));

                return newPolyhedron.Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating polyhedron: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Retrieves polyhedron dimensions from the database for a given object name.
        /// Creates necessary tables if they don't exist and populates collection with retrieved data.
        /// </summary>
        /// <param name="dbConnection">Database connection for data retrieval</param>
        /// <param name="objectName">Name of the object to retrieve polyhedron dimensions for</param>
        /// <returns>Observable collection of polyhedron dimensions</returns>
        public static async Task<ObservableCollection<PolyhedronDimensions>> GetDimensionPolyhedronPartsAsync(
            SqliteConnection dbConnection,
            string objectName)
        {
            // Ensure the polyhedron table exists
            await PolyhedronDimensionsExtensions.CreateTable(dbConnection);

            // Get records from database by object name
            var records = await new PolyhedronDimensions().GetByObjectNameAsync(dbConnection, objectName);

            // Return as ObservableCollection
            return new ObservableCollection<PolyhedronDimensions>(records);
        }
        /// <summary>
        /// Generates OpenSCAD code for polyhedron points, faces, and module definition.
        /// Creates point and face arrays inside a module, sorted by PointsId and FaceId respectively.
        /// Uses the maximum convexity value from all polyhedron dimensions.
        /// </summary>
        /// <param name="objectName">Name of the polyhedron object</param>
        /// <param name="polyhedronDimensions">Collection of polyhedron dimensions containing points and faces</param>
        /// <param name="convexity">Default convexity parameter for the polyhedron (overridden by max from collection)</param>
        /// <returns>OpenSCAD code string defining module with points, faces, and polyhedron call</returns>
        public static string GenerateOSCADShapeAsync(
            string objectName,
            ObservableCollection<PolyhedronDimensions> polyhedronDimensions,
            int convexity = 1)
        {
            try
            {
                var scadCode = new System.Text.StringBuilder();
                var sanitizedName = SanitizeNameForOpenSCAD(objectName);

                // Calculate polyhedron dimensions
                var (length, width, height) = CalculatePolyhedronDimensions(polyhedronDimensions);

                // Add dimension variables at the top
                scadCode.Append(GeneratePolyhedronDimensionVariables(objectName, length, width, height));

                // Separate points and faces
                var pointsList = polyhedronDimensions
                    .Where(p => p.PolyhedronOperationType == "Points")
                    .OrderBy(p => p.PointsId)
                    .ToList();

                var facesList = polyhedronDimensions
                    .Where(p => p.PolyhedronOperationType == "Faces")
                    .OrderBy(p => p.FaceId)
                    .ToList();

                // Get the maximum convexity from all polyhedron dimensions
                int maxConvexity = polyhedronDimensions.Any() 
                    ? polyhedronDimensions.Max(p => p.Convexity) 
                    : convexity;

                // Generate module definition
                scadCode.AppendLine($"module {sanitizedName}_polyhedron()");
                scadCode.AppendLine("{");

                // Generate points array inside module
                if (pointsList.Count > 0)
                {
                    scadCode.AppendLine($"  {sanitizedName}_points = [");
                    for (int i = 0; i < pointsList.Count; i++)
                    {
                        var point = pointsList[i];
                        string comment = string.IsNullOrWhiteSpace(point.Description)
                            ? $"    // PointsId: {point.PointsId}"
                            : $"    // {point.Description} PointsId: {point.PointsId}";

                        scadCode.Append($"      [{point.PointX_MM},{point.PointY_MM},{point.PointZ_MM}]");

                        if (i < pointsList.Count - 1)
                            scadCode.Append(",");

                        scadCode.AppendLine($" {comment}");
                    }
                    scadCode.AppendLine("   ];");
                    scadCode.AppendLine();
                }

                // Generate faces array inside module
                if (facesList.Count > 0)
                {
                    scadCode.AppendLine($"  {sanitizedName}_faces = [");
                    for (int i = 0; i < facesList.Count; i++)
                    {
                        var face = facesList[i];
                        string comment = string.IsNullOrWhiteSpace(face.Description)
                            ? $"    // FaceId: {face.FaceId}"
                            : $"    // {face.Description} FaceId: {face.FaceId}";

                        scadCode.Append($"      {face.Face}");

                        if (i < facesList.Count - 1)
                            scadCode.Append(",");

                        scadCode.AppendLine($" {comment}");
                    }
                    scadCode.AppendLine("   ];");
                    scadCode.AppendLine();
                }

                // Generate polyhedron call inside module if both points and faces exist
                if (pointsList.Count > 0 && facesList.Count > 0)
                {
                    scadCode.AppendLine($"    polyhedron(points = {sanitizedName}_points, faces = {sanitizedName}_faces, convexity = {maxConvexity});");
                }

                scadCode.AppendLine("}");
                scadCode.AppendLine();

                System.Diagnostics.Debug.WriteLine(
                    $"Generated OpenSCAD shape: {sanitizedName} with {pointsList.Count} points and {facesList.Count} faces (max convexity: {maxConvexity})");

                return scadCode.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating OpenSCAD shape: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Sanitizes object names for use in OpenSCAD code.
        /// Converts to lowercase and replaces non-alphanumeric characters with underscores.
        /// </summary>
        /// <param name="objectName">The name to sanitize</param>
        /// <returns>Sanitized name safe for OpenSCAD</returns>
        private static string SanitizeNameForOpenSCAD(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return "shape";

            // Convert to lowercase and replace non-alphanumeric characters with underscores
            var sanitized = System.Text.RegularExpressions.Regex.Replace(
                objectName.Trim().ToLower(),
                @"[^a-z0-9]+",
                "_");

            // Remove leading/trailing underscores
            sanitized = sanitized.Trim('_');

            return string.IsNullOrEmpty(sanitized) ? "shape" : sanitized;
        }

        /// <summary>
        /// Writes polyhedron shape SCAD code to a preview file with module call.
        /// Creates a temporary preview file in the Solids directory for viewing.
        /// </summary>
        /// <param name="shapeName">Name of the shape (used for filename)</param>
        /// <param name="shapeScadCode">The generated SCAD code containing points, faces, and module definition</param>
        /// <param name="objectFilePath">Base SCAD path from IScadPathProvider</param>
        /// <returns>Full path to the created preview file</returns>
        public static async Task<string> ShapeToScadPreviewAsync(
            string shapeName,
            string shapeScadCode,
            string objectFilePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(shapeName))
                {
                    System.Diagnostics.Debug.WriteLine("Error: Shape name cannot be null or empty");
                    return string.Empty;
                }

                if (string.IsNullOrWhiteSpace(shapeScadCode))
                {
                    System.Diagnostics.Debug.WriteLine("Error: SCAD code cannot be null or empty");
                    return string.Empty;
                }

                if (string.IsNullOrWhiteSpace(objectFilePath))
                {
                    System.Diagnostics.Debug.WriteLine("Error: SCAD path cannot be null or empty");
                    return string.Empty;
                }

                // Ensure the Solids directory exists
                var solidsPath = Path.Combine(objectFilePath, "Solids");
                if (!Directory.Exists(solidsPath))
                {
                    Directory.CreateDirectory(solidsPath);
                }

                // Create preview filename
                var sanitizedName = SanitizeNameForOpenSCAD(shapeName);
                var previewFileName = $"{sanitizedName}_shape.scad";
                var previewFilePath = Path.Combine(solidsPath, previewFileName);

                // Generate preview code with module call appended
                var previewCode = GenerateOSCADShapePreviewCode(shapeName, shapeScadCode);

                // Write the preview SCAD code to file
                await File.WriteAllTextAsync(previewFilePath, previewCode);

                System.Diagnostics.Debug.WriteLine(
                    $"Shape preview SCAD file created: {previewFilePath}");
                return previewFilePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating shape preview: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Opens a polyhedron shape preview SCAD file in OpenSCAD.
        /// </summary>
        /// <param name="previewFilePath">Full path to the SCAD preview file</param>
        /// <param name="allowDuplicates">If false, prevents opening the file if it's already open. If true, allows multiple instances.</param>
        /// <returns>Task representing the async process operation</returns>
        public static async Task OpenShapePreviewAsync(string previewFilePath, bool allowDuplicates = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(previewFilePath))
                {
                    System.Diagnostics.Debug.WriteLine("Error: Preview file path cannot be null or empty");
                    return;
                }

                if (!File.Exists(previewFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Error: Preview file not found: {previewFilePath}");
                    return;
                }

                // Open the file in OpenSCAD with duplicate control
                var openScadSuccess = await ScadFileOperations.OpenScadFileAsync(previewFilePath, allowDuplicates: allowDuplicates);

                if (!openScadSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to open SCAD file: {previewFilePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening shape preview: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates OpenSCAD preview code that includes the shape module definition
        /// and appends a call to use it. This is for preview purposes only.
        /// </summary>
        /// <param name="objectName">Name of the polyhedron object</param>
        /// <param name="shapeScadCode">The generated SCAD code containing the module definition</param>
        /// <returns>OpenSCAD code string with module definition and preview call</returns>
        public static string GenerateOSCADShapePreviewCode(
            string objectName,
            string shapeScadCode)
        {
            try
            {
                var previewCode = new System.Text.StringBuilder();

                // Add the module definition
                previewCode.Append(shapeScadCode);

                // Add the preview call
                var sanitizedName = SanitizeNameForOpenSCAD(objectName);
                previewCode.AppendLine();
                previewCode.AppendLine("// Preview - Add to an object module");
                previewCode.AppendLine($"{sanitizedName}_polyhedron();");

                System.Diagnostics.Debug.WriteLine(
                    $"Generated OpenSCAD shape preview code for: {sanitizedName}");

                return previewCode.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating shape preview code: {ex.Message}");
                return shapeScadCode; // Return original if error
            }
        }

        /// <summary>
        /// Calculates the bounding box dimensions (length, width, height) of a polyhedron.
        /// Uses only the points that are referenced in faces to determine the bounds.
        /// X-axis difference = Length, Y-axis difference = Width, Z-axis difference = Height.
        /// </summary>
        /// <param name="polyhedronDimensions">Collection of polyhedron dimensions containing points and faces</param>
        /// <returns>Tuple containing (Length, Width, Height) in millimeters, or (0, 0, 0) if no valid points found</returns>
        public static (double Length, double Width, double Height) CalculatePolyhedronDimensions(
            ObservableCollection<PolyhedronDimensions> polyhedronDimensions)
        {
            try
            {
                // Get all points
                var pointsList = polyhedronDimensions
                    .Where(p => p.PolyhedronOperationType == "Points")
                    .OrderBy(p => p.PointsId)
                    .ToList();

                // Get all faces
                var facesList = polyhedronDimensions
                    .Where(p => p.PolyhedronOperationType == "Faces")
                    .OrderBy(p => p.FaceId)
                    .ToList();

                if (pointsList.Count == 0 || facesList.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Warning: No points or faces found for dimension calculation");
                    return (0, 0, 0);
                }

                // Extract all point indices used in faces
                var usedPointIndices = new HashSet<int>();
                var faceIndexPattern = new System.Text.RegularExpressions.Regex(@"\d+");

                foreach (var face in facesList)
                {
                    if (string.IsNullOrWhiteSpace(face.Face))
                        continue;

                    // Extract all numbers from face definition like [0,1,2] or [0,3,1,4]
                    var matches = faceIndexPattern.Matches(face.Face);
                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        if (int.TryParse(match.Value, out var index))
                        {
                            usedPointIndices.Add(index);
                        }
                    }
                }

                if (usedPointIndices.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Warning: No point indices found in faces");
                    return (0, 0, 0);
                }

                // Get only the points used in faces
                var usedPoints = pointsList
                    .Where(p => usedPointIndices.Contains(p.PointsId))
                    .ToList();

                if (usedPoints.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Warning: No valid points found in used indices");
                    return (0, 0, 0);
                }

                // Calculate min and max for each axis
                double minX = usedPoints.Min(p => p.PointX_MM);
                double maxX = usedPoints.Max(p => p.PointX_MM);
                double minY = usedPoints.Min(p => p.PointY_MM);
                double maxY = usedPoints.Max(p => p.PointY_MM);
                double minZ = usedPoints.Min(p => p.PointZ_MM);
                double maxZ = usedPoints.Max(p => p.PointZ_MM);

                // Calculate dimensions (difference between max and min)
                double length = Math.Round(maxX - minX, 2); // X-axis
                double width = Math.Round(maxY - minY, 2);   // Y-axis
                double height = Math.Round(maxZ - minZ, 2);  // Z-axis

                System.Diagnostics.Debug.WriteLine(
                    $"Polyhedron dimensions calculated - Length: {length}mm, Width: {width}mm, Height: {height}mm");

                return (length, width, height);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating polyhedron dimensions: {ex.Message}");
                return (0, 0, 0);
            }
        }

        /// <summary>
        /// Generates OpenSCAD variable definitions for polyhedron dimensions.
        /// Creates variables for length, width, and height based on bounding box calculations.
        /// </summary>
        /// <param name="objectName">Name of the polyhedron object</param>
        /// <param name="length">Length in millimeters (X-axis)</param>
        /// <param name="width">Width in millimeters (Y-axis)</param>
        /// <param name="height">Height in millimeters (Z-axis)</param>
        /// <returns>OpenSCAD variable definition string</returns>
        public static string GeneratePolyhedronDimensionVariables(
            string objectName,
            double length,
            double width,
            double height)
        {
            try
            {
                var variablesCode = new System.Text.StringBuilder();
                var sanitizedName = SanitizeNameForOpenSCAD(objectName);

                variablesCode.AppendLine("// Polyhedron Dimensions");
                variablesCode.AppendLine($"{sanitizedName}_length = {length}; // X-axis");
                variablesCode.AppendLine($"{sanitizedName}_width = {width};   // Y-axis");
                variablesCode.AppendLine($"{sanitizedName}_height = {height}; // Z-axis");
                variablesCode.AppendLine();

                System.Diagnostics.Debug.WriteLine(
                    $"Generated dimension variables for: {sanitizedName}");

                return variablesCode.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating dimension variables: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
