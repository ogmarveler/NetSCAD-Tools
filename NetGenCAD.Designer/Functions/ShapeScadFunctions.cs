using Microsoft.Data.Sqlite;
using NetGenCAD.Designer.Repositories;
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
            // Ensure the polyhedron and shape dimension tables exists if not already created in the database
            await PolyhedronDimensionsExtensions.CreateTable(dbConnection);
            await ShapeDimensionsExtensions.CreateTable(dbConnection);

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

                // Separate points and faces
                var pointsList = polyhedronDimensions
                    .Where(p => p.PolyhedronOperationType == "Points")
                    .OrderBy(p => p.PointsId)
                    .ToList();

                var facesList = polyhedronDimensions
                    .Where(p => p.PolyhedronOperationType == "Faces")
                    .OrderBy(p => p.FaceId)
                    .ToList();

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
                    scadCode.AppendLine($"    polyhedron(points = {sanitizedName}_points, faces = {sanitizedName}_faces, convexity = {convexity});");
                }

                scadCode.AppendLine("}");
                scadCode.AppendLine();

                System.Diagnostics.Debug.WriteLine(
                    $"Generated OpenSCAD shape: {sanitizedName} with {pointsList.Count} points and {facesList.Count} faces");

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
            string objectFilePath,
            UnitSystem axisUnit,
            ObservableCollection<PolyhedronDimensions> polyhedronDimensions,
            AxisDimensions? axisDimensions = null,
            double? axisXPositionMM = null,
            double? axisYPositionMM = null,
            double? axisZPositionMM = null)
        {
            try
            {
                if(axisUnit == UnitSystem.Imperial)
                {
                    axisXPositionMM = Math.Round(InchesToMillimeter((double)axisXPositionMM!), ShapeDimensions.OpenSCAD_DecimalPlaces);
                    axisYPositionMM = Math.Round(InchesToMillimeter((double)axisYPositionMM!), ShapeDimensions.OpenSCAD_DecimalPlaces);
                    axisZPositionMM = Math.Round(InchesToMillimeter((double)axisZPositionMM!), ShapeDimensions.OpenSCAD_DecimalPlaces);
                }

                if (string.IsNullOrWhiteSpace(shapeName) || string.IsNullOrWhiteSpace(shapeScadCode) || string.IsNullOrWhiteSpace(objectFilePath))
                {
                    System.Diagnostics.Debug.WriteLine("Error: Invalid parameters");
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

                // Generate preview code with module call and dimensions appended
                var previewCode = GenerateOSCADShapePreviewCode(
                    shapeName,
                    shapeScadCode,
                    polyhedronDimensions,
                    axisDimensions,
                    axisXPositionMM,
                    axisYPositionMM,
                    axisZPositionMM);

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
        /// Generates OpenSCAD preview code that includes the shape module definition,
        /// polyhedron dimensions, and appends a call to use it. This is for preview purposes only.
        /// </summary>
        /// <param name="objectName">Name of the polyhedron object</param>
        /// <param name="shapeScadCode">The generated SCAD code containing the module definition</param>
        /// <param name="polyhedronDimensions">Collection of polyhedron dimensions for calculating bounding box</param>
        /// <returns>OpenSCAD code string with dimensions, module definition, and preview call</returns>
        public static string GenerateOSCADShapePreviewCode(
            string objectName,
            string shapeScadCode,
            ObservableCollection<PolyhedronDimensions> polyhedronDimensions,
            AxisDimensions? axisDimensions = null,
            double? axisXPositionMM = null,
            double? axisYPositionMM = null,
            double? axisZPositionMM = null)
        {
            try
            {
                var previewCode = new System.Text.StringBuilder();

                // Calculate polyhedron dimensions for preview
                var (length, width, height) = CalculatePolyhedronDimensions(polyhedronDimensions);

                // Add dimension variables at the top
                previewCode.Append(GeneratePolyhedronDimensionVariables(objectName, length, width, height));

                // Add axis if provided
                if (axisDimensions != null && axisXPositionMM.HasValue && axisYPositionMM.HasValue && axisZPositionMM.HasValue)
                {
                    previewCode.AppendLine("// Custom axis");
                    previewCode.AppendLine(axisDimensions.IncludeMethod);
                    var wrappedAxisCall = $"translate ([{axisXPositionMM}, {axisYPositionMM}, {axisZPositionMM}]) {axisDimensions.OSCADMethod.Replace(axisDimensions.IncludeMethod, "")}";
                    previewCode.AppendLine(wrappedAxisCall);
                    previewCode.AppendLine();
                }

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
                return shapeScadCode;
            }
        }

        /// <summary>
        /// Calculates the bounding box dimensions (length, width, height) of a polyhedron.
        /// Uses only the points that are referenced in faces to determine the bounds.
        /// X-axis difference = Length, Y-axis difference = Width, Z-axis difference = Height.
        /// </summary>
        /// <param name="polyhedronDimensions">Collection of polyhedron dimensions containing points and faces</param>
        /// <returns>Tuple containing (Length, Width, Height) in millimeters, or (0, 0, 0) if no valid points found</returns>
        public static (double Length, double Width, double Height) CalculatePolyhedronDimensions(ObservableCollection<PolyhedronDimensions> polyhedronDimensions)
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
        public static string GeneratePolyhedronDimensionVariables(string objectName,double length,double width,double height)
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

        /// <summary>
        /// Stores a polyhedron shape into the ShapeDimensions repository with calculated metadata.
        /// Calculates bounding box dimensions, vertex/face/edge counts, convexity, and volume.
        /// Minifies the OpenSCAD code by removing comments and unnecessary whitespace.
        /// </summary>
        /// <param name="shapeName">Name of the shape</param>
        /// <param name="shapeDescription">Description of the shape</param>
        /// <param name="shapeScadCode">The full OpenSCAD module code to store</param>
        /// <param name="polyhedronDimensions">Collection of polyhedron dimensions (points and faces)</param>
        /// <param name="dbConnection">Database connection for persistence</param>
        /// <returns>The ID of the created ShapeDimensions record, or 0 if creation failed</returns>
        public static async Task<int> CreateNewShapeModuleAsync(string shapeName,string shapeDescription,string shapeScadCode,ObservableCollection<PolyhedronDimensions> polyhedronDimensions,SqliteConnection dbConnection, int convexity = 1)
        {
            try
            {
                // Calculate bounding box dimensions from polyhedron points
                var (length, width, height) = CalculatePolyhedronDimensions(polyhedronDimensions);

                // Calculate surface area
                var (surfaceAreaCM2, surfaceAreaIN2) = CalculatePolyhedronSurfaceArea(polyhedronDimensions);

                // Count vertices (points) and faces
                var pointsList = polyhedronDimensions
                    .Where(p => p.PolyhedronOperationType == "Points")
                    .ToList();

                var facesList = polyhedronDimensions
                    .Where(p => p.PolyhedronOperationType == "Faces")
                    .ToList();

                int numberOfVertices = pointsList.Count;
                int numberOfFaces = facesList.Count;

                // Calculate number of edges using Euler's formula: V - E + F = 2
                // Therefore: E = V + F - 2
                int numberOfEdges = numberOfVertices + numberOfFaces - 2;

                // Calculate volume in cubic millimeters, then convert to cm³
                // Volume ≈ (length × width × height) / 6 for a rough polyhedron estimate
                double volumeMM3 = (length * width * height) / 6.0;
                double volumeCM3 = volumeMM3 / 1000.0; // Convert mm³ to cm³
                double volumeIN3 = volumeCM3 / 16.387064; // Convert cm³ to in³

                // Minify the OpenSCAD code
                string minifiedScadCode = MinifyOpenScadCode(shapeScadCode);

                // Create ShapeDimensions entity
                var shapeDimensions = new ShapeDimensions
                {
                    Name = shapeName,
                    Description = shapeDescription,
                    BoxLength_MM = length,
                    BoxWidth_MM = width,
                    BoxHeight_MM = height,
                    NumberOfVertices = numberOfVertices,
                    NumberOfFaces = numberOfFaces,
                    NumberOfEdges = numberOfEdges,
                    Convexity = convexity,
                    Volume_CM3 = Math.Round(volumeCM3, ShapeDimensions.OpenSCAD_DecimalPlaces),
                    Volume_IN3 = Math.Round(volumeIN3, ShapeDimensions.OpenSCAD_DecimalPlaces),
                    SurfaceArea_CM2 = surfaceAreaCM2,
                    SurfaceArea_IN2 = surfaceAreaIN2,
                    OSCADMethod = minifiedScadCode,
                    CreatedAt = DateTime.UtcNow
                };

                // Ensure the ShapeDimensions table exists
                await ShapeDimensionsExtensions.CreateTable(dbConnection);

                // Upsert into database (insert if new, update if Name already exists)
                int shapeId = await shapeDimensions.UpsertAsync(dbConnection);

                System.Diagnostics.Debug.WriteLine(
                    $"Shape created successfully: ID={shapeId}, Name={shapeName}, Vertices={numberOfVertices}, Faces={numberOfFaces}, Volume={volumeCM3:F2}cm³, SurfaceArea={surfaceAreaCM2:F2}cm²");

                return shapeId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating shape module: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Minifies OpenSCAD code by removing comments, extra whitespace, and unnecessary formatting.
        /// Preserves functional code while reducing file size.
        /// </summary>
        /// <param name="scadCode">The OpenSCAD code to minify</param>
        /// <returns>Minified OpenSCAD code</returns>
        private static string MinifyOpenScadCode(string scadCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(scadCode))
                    return scadCode;

                var lines = scadCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var minifiedLines = new List<string>();

                foreach (var line in lines)
                {
                    // Remove comments (both // and /* */ style)
                    string trimmedLine = line;

                    // Remove // comments
                    int commentIndex = trimmedLine.IndexOf("//");
                    if (commentIndex >= 0)
                    {
                        trimmedLine = trimmedLine.Substring(0, commentIndex);
                    }

                    // Trim whitespace
                    trimmedLine = trimmedLine.Trim();

                    // Only add non-empty lines
                    if (!string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        minifiedLines.Add(trimmedLine);
                    }
                }

                // Join lines with minimal spacing and remove unnecessary whitespace around operators
                string minified = string.Join("", minifiedLines);

                // Remove extra spaces around common operators and brackets
                minified = System.Text.RegularExpressions.Regex.Replace(minified, @"\s+", " ");
                minified = System.Text.RegularExpressions.Regex.Replace(minified, @"\s*([{};,\[\]=])\s*", "$1");
                minified = minified.Replace("( ", "(").Replace(" )", ")").Replace("[ ", "[").Replace(" ]", "]");

                System.Diagnostics.Debug.WriteLine(
                    $"OpenSCAD code minified: {scadCode.Length} bytes -> {minified.Length} bytes");

                return minified;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error minifying OpenSCAD code: {ex.Message}");
                return scadCode; // Return original if minification fails
            }
        }

        /// <summary>
        /// Calculates the surface area of a polyhedron based on its points and faces.
        /// Uses the shoelace formula for each triangular or polygonal face.
        /// </summary>
        /// <param name="polyhedronDimensions">Collection of polyhedron dimensions containing points and faces</param>
        /// <returns>Tuple containing (SurfaceArea_CM2, SurfaceArea_IN2)</returns>
        public static (double SurfaceAreaCM2, double SurfaceAreaIN2) CalculatePolyhedronSurfaceArea(
            ObservableCollection<PolyhedronDimensions> polyhedronDimensions)
        {
            try
            {
                // Get all points indexed by PointsId
                var pointsDict = polyhedronDimensions
                    .Where(p => p.PolyhedronOperationType == "Points")
                    .OrderBy(p => p.PointsId)
                    .ToDictionary(p => p.PointsId, p => (p.PointX_MM, p.PointY_MM, p.PointZ_MM));

                // Get all faces
                var facesList = polyhedronDimensions
                    .Where(p => p.PolyhedronOperationType == "Faces")
                    .OrderBy(p => p.FaceId)
                    .ToList();

                if (pointsDict.Count == 0 || facesList.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Warning: No points or faces found for surface area calculation");
                    return (0, 0);
                }

                double totalSurfaceAreaMM2 = 0;

                // Calculate area for each face
                foreach (var face in facesList)
                {
                    if (string.IsNullOrWhiteSpace(face.Face))
                        continue;

                    // Parse face indices from format like [0,1,2],[2,3,0]
                    var faceIndices = ExtractFaceIndices(face.Face);

                    if (faceIndices.Count < 3)
                        continue;

                    // Calculate area of the polygon using shoelace formula (for planar faces)
                    double faceAreaMM2 = CalculateFaceArea(faceIndices, pointsDict);
                    totalSurfaceAreaMM2 += faceAreaMM2;
                }

                // Convert from mm² to cm² (1 cm² = 100 mm²)
                double surfaceAreaCM2 = totalSurfaceAreaMM2 / 100.0;

                // Convert from cm² to in² (1 in² = 6.4516 cm²)
                double surfaceAreaIN2 = surfaceAreaCM2 / 6.4516;

                System.Diagnostics.Debug.WriteLine(
                    $"Polyhedron surface area calculated - {surfaceAreaCM2:F2}cm² ({surfaceAreaIN2:F2}in²)");

                return (
                    Math.Round(surfaceAreaCM2, ShapeDimensions.OpenSCAD_DecimalPlaces),
                    Math.Round(surfaceAreaIN2, ShapeDimensions.OpenSCAD_DecimalPlaces)
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating surface area: {ex.Message}");
                return (0, 0);
            }
        }

        /// <summary>
        /// Extracts face indices from a face definition string like [0,1,2]
        /// </summary>
        private static List<int> ExtractFaceIndices(string faceDefinition)
        {
            var indices = new List<int>();
            try
            {
                // Parse indices from format like [0,1,2]
                var matches = System.Text.RegularExpressions.Regex.Matches(faceDefinition, @"\d+");
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (int.TryParse(match.Value, out var index))
                    {
                        indices.Add(index);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting face indices: {ex.Message}");
            }

            return indices;
        }

        /// <summary>
        /// Calculates the area of a single polygonal face using the shoelace formula.
        /// Assumes the face is planar.
        /// </summary>
        private static double CalculateFaceArea(List<int> faceIndices, Dictionary<int, (double PointX_MM, double PointY_MM, double PointZ_MM)> pointsDict)
        {
            try
            {
                if (faceIndices.Count < 3)
                    return 0;

                // Get the points for this face
                var facePoints = new List<(double x, double y, double z)>();
                foreach (var index in faceIndices)
                {
                    if (pointsDict.TryGetValue(index, out var point))
                    {
                        facePoints.Add((point.PointX_MM, point.PointY_MM, point.PointZ_MM));
                    }
                }

                if (facePoints.Count < 3)
                    return 0;

                // For a planar polygon, calculate area using cross product method
                // Area = 0.5 * |sum of cross products|
                double area = 0;

                // Use the first point as reference
                for (int i = 1; i < facePoints.Count - 1; i++)
                {
                    // Vector from point 0 to point i
                    double v1x = facePoints[i].x - facePoints[0].x;
                    double v1y = facePoints[i].y - facePoints[0].y;
                    double v1z = facePoints[i].z - facePoints[0].z;

                    // Vector from point 0 to point i+1
                    double v2x = facePoints[i + 1].x - facePoints[0].x;
                    double v2y = facePoints[i + 1].y - facePoints[0].y;
                    double v2z = facePoints[i + 1].z - facePoints[0].z;

                    // Cross product
                    double crossX = v1y * v2z - v1z * v2y;
                    double crossY = v1z * v2x - v1x * v2z;
                    double crossZ = v1x * v2y - v1y * v2x;

                    // Magnitude of cross product
                    double magnitude = Math.Sqrt(crossX * crossX + crossY * crossY + crossZ * crossZ);
                    area += magnitude * 0.5;
                }

                return area;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating face area: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Retrieves ShapeDimensions records from the database matching the given shape name.
        /// </summary>
        /// <param name="shapeName">Name of the shape to retrieve</param>
        /// <param name="dbConnection">Database connection</param>
        /// <returns>Collection of ShapeDimensions matching the shape name</returns>
        public static async Task<IEnumerable<ShapeDimensions>> GetShapeDimensionsByNameAsync(string shapeName,SqliteConnection dbConnection)
        {
            try
            {
                // Ensure the ShapeDimensions table exists
                await ShapeDimensionsExtensions.CreateTable(dbConnection);

                // Retrieve shapes matching the name
                var shapes = await new ShapeDimensions().GetByNameAsync(dbConnection, shapeName);

                System.Diagnostics.Debug.WriteLine(
                    $"Retrieved {shapes.Count()} shape dimensions for: {shapeName}");

                return shapes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving shape dimensions: {ex.Message}");
                return new List<ShapeDimensions>();
            }
        }

        /// <summary>
        /// Updates all SolidDimensions rows that reference a shape (via ShapeName) with new polyhedron data and SCAD code.
        /// Updates dimensions, volumes, and the OSCADMethod with the new shape body while preserving rotation/translation/color.
        /// Retrieves imperial dimensions from ShapeDimensions and applies them to all matching SolidDimensions rows.
        /// </summary>
        /// <param name="shapeName">The name of the shape to update (matches ShapeDimensions.Name and SolidDimensions.ShapeName)</param>
        /// <param name="newShapeScadCode">The new complete SCAD code from the updated ShapeDimensions</param>
        /// <param name="boxLengthMM">New bounding box length in millimeters</param>
        /// <param name="boxWidthMM">New bounding box width in millimeters</param>
        /// <param name="boxHeightMM">New bounding box height in millimeters</param>
        /// <param name="volumeCM3">New volume in cubic centimeters</param>
        /// <param name="boxLengthIN">New bounding box length in inches</param>
        /// <param name="boxWidthIN">New bounding box width in inches</param>
        /// <param name="boxHeightIN">New bounding box height in inches</param>
        /// <param name="volumeIN3">New volume in cubic inches</param>
        /// <param name="dbConnection">Database connection</param>
        /// <returns>Number of rows updated</returns>
        public static async Task<int> UpdateSolidDimensionsWithShapeAsync(
            string shapeName,
            string newShapeScadCode,
            double boxLengthMM,
            double boxWidthMM,
            double boxHeightMM,
            double volumeCM3,
            double boxLengthIN,
            double boxWidthIN,
            double boxHeightIN,
            double volumeIN3,
            SqliteConnection dbConnection)
        {
            try
            {
                // Extract the module body (content between braces)
                string shapeBody = ObjectScadFunctions.ExtractModuleBody(newShapeScadCode);

                // Get the ShapeDimensions record to retrieve imperial values
                var shapeDimensions = await GetShapeDimensionsByNameAsync(shapeName, dbConnection);
                var shape = shapeDimensions.FirstOrDefault();

                if (shape == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: ShapeDimensions not found for shape name: {shapeName}");
                    return 0;
                }

                // SQL UPDATE statement that:
                // 1. Updates metric and imperial dimension fields from ShapeDimensions
                // 2. Replaces the shape body inside the rotation/translation/color wrappers
                string updateSql = @"
                    UPDATE SolidDimensions
                    SET 
                        Length_MM = @BoxLengthMM,
                        Width_MM = @BoxWidthMM,
                        Height_MM = @BoxHeightMM,
                        Volume_CM3 = @VolumeCM3,
                        Length_IN = @BoxLengthIN,
                        Width_IN = @BoxWidthIN,
                        Height_IN = @BoxHeightIN,
                        Volume_IN3 = @VolumeIN3,
                        OSCADMethod = 
                            CONCAT(
                                SUBSTR(
                                    OSCADMethod,1,INSTR(SUBSTR(OSCADMethod, INSTR(SUBSTR(OSCADMethod, INSTR(OSCADMethod, '{') + 1), '{') + INSTR(OSCADMethod, '{') + 1),'{') + INSTR(SUBSTR(OSCADMethod, INSTR(OSCADMethod, '{') + 1), '{') + INSTR(OSCADMethod, '{')),
                                @ShapeBody,
                                '}} }')
                        WHERE ShapeName = @ShapeName
                        AND SolidType = 'Polyhedron'";

                var parameters = new List<(string, object)>
                {
                    ("@BoxLengthMM", boxLengthMM),
                    ("@BoxWidthMM", boxWidthMM),
                    ("@BoxHeightMM", boxHeightMM),
                    ("@VolumeCM3", volumeCM3),
                    ("@BoxLengthIN", boxLengthIN),
                    ("@BoxWidthIN", boxWidthIN),
                    ("@BoxHeightIN", boxHeightIN),
                    ("@VolumeIN3", volumeIN3),
                    ("@ShapeBody", shapeBody),
                    ("@ShapeName", shapeName)
                };

                var cmd = dbConnection.CreateCommand();
                cmd.CommandText = updateSql;

                foreach (var (paramName, paramValue) in parameters)
                {
                    cmd.Parameters.AddWithValue(paramName, paramValue ?? DBNull.Value);
                }

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                System.Diagnostics.Debug.WriteLine(
                    $"Updated {rowsAffected} SolidDimensions rows for shape: {shapeName}");

                return rowsAffected;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating solid dimensions with shape: {ex.Message}");
                return 0;
            }
        }
    }
}
