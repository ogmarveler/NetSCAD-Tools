using NetGenCAD.Designer.Repositories;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using static NetGenCAD.Core.Measurements.Conversion;
using static NetGenCAD.Core.Measurements.Selector;

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
            // Ensure the polyhedron table exists
            await PolyhedronDimensionsExtensions.CreateTable(dbConnection);

            // Get records from database by object name
            var records = await new PolyhedronDimensions().GetByObjectNameAsync(dbConnection, objectName);

            // Return as ObservableCollection
            return new ObservableCollection<PolyhedronDimensions>(records);
        }
    }
}
