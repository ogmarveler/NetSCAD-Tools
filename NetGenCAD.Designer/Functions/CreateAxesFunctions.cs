using NetGenCAD.Axis.Scad.Models;
using NetGenCAD.Axis.Scad.Utility;
using NetGenCAD.Axis.SCAD.Modules;
using NetGenCAD.Core.Measurements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static NetGenCAD.Axis.SCAD.Utility.AxisConfig;
using static NetGenCAD.Core.Measurements.Conversion;
using static NetGenCAD.Core.Measurements.Selector;

namespace NetGenCAD.Designer.Functions
{
    /// <summary>
    /// Static utility functions for axis creation and management.
    /// Extracted from CreateAxesViewModel for separation of concerns and reusability.
    /// All functions are stateless and designed for use across multiple ViewModels.
    /// </summary>
    public static class CreateAxesFunctions
    {
        /// <summary>
        /// Retrieves and filters axes from the axes.scad file based on unit system.
        /// Parses the axes.scad file and separates metrics from imperial axes.
        /// </summary>
        /// <returns>Tuple containing: all axes, metric-only axes, imperial-only axes</returns>
        public static (
            ObservableCollection<GeneratedModule> AllAxes,
            ObservableCollection<GeneratedModule> MetricAxes,
            ObservableCollection<GeneratedModule> ImperialAxes) GetAxesList()
        {
            var parser = new ScadParser();
            var filePath = Path.Combine("Scad", "Axes", "axes.scad");
            var allAxes = parser.AxesModulesList(filePath);

            var metricAxes = new ObservableCollection<GeneratedModule>(
                allAxes.Where(x => x.CallingMethod != null && x.CallingMethod.Contains("_MM_")));

            var imperialAxes = new ObservableCollection<GeneratedModule>(
                allAxes.Where(x => x.CallingMethod != null && x.CallingMethod.Contains("_Inch_")));

            return (allAxes, metricAxes, imperialAxes);
        }

        /// <summary>
        /// Converts axis range values from metric (mm) to imperial (inches).
        /// Applies unit conversion to all axis coordinates and volume measurements.
        /// </summary>
        /// <param name="minX">Minimum X value in millimeters</param>
        /// <param name="maxX">Maximum X value in millimeters</param>
        /// <param name="minY">Minimum Y value in millimeters</param>
        /// <param name="maxY">Maximum Y value in millimeters</param>
        /// <param name="minZ">Minimum Z value in millimeters</param>
        /// <param name="maxZ">Maximum Z value in millimeters</param>
        /// <param name="volume">Volume in cubic centimeters</param>
        /// <param name="volumeScale">Volume scale in cubic meters</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <returns>Tuple containing converted values: minX, maxX, minY, maxY, minZ, maxZ, volume, volumeScale (all in imperial units)</returns>
        public static (double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ, double Volume, double VolumeScale) 
            ConvertToImperial(
                double minX, double maxX,
                double minY, double maxY,
                double minZ, double maxZ,
                double volume, double volumeScale,
                int decimalPlaces)
        {
            return (
                Math.Round(MillimeterToInches(minX), decimalPlaces),
                Math.Round(MillimeterToInches(maxX), decimalPlaces),
                Math.Round(MillimeterToInches(minY), decimalPlaces),
                Math.Round(MillimeterToInches(maxY), decimalPlaces),
                Math.Round(MillimeterToInches(minZ), decimalPlaces),
                Math.Round(MillimeterToInches(maxZ), decimalPlaces),
                Math.Round(VolumeConverter.ConvertCm3ToIn3(volume), decimalPlaces),
                Math.Round(VolumeConverter.ConvertM3ToFt3(volumeScale), decimalPlaces)
            );
        }

        /// <summary>
        /// Converts axis range values from imperial (inches) to metric (mm).
        /// Applies unit conversion to all axis coordinates and volume measurements.
        /// </summary>
        /// <param name="minX">Minimum X value in inches</param>
        /// <param name="maxX">Maximum X value in inches</param>
        /// <param name="minY">Minimum Y value in inches</param>
        /// <param name="maxY">Maximum Y value in inches</param>
        /// <param name="minZ">Minimum Z value in inches</param>
        /// <param name="maxZ">Maximum Z value in inches</param>
        /// <param name="volume">Volume in cubic inches</param>
        /// <param name="volumeScale">Volume scale in cubic feet</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <returns>Tuple containing converted values: minX, maxX, minY, maxY, minZ, maxZ, volume, volumeScale (all in metric units)</returns>
        public static (double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ, double Volume, double VolumeScale)
            ConvertToMetric(
                double minX, double maxX,
                double minY, double maxY,
                double minZ, double maxZ,
                double volume, double volumeScale,
                int decimalPlaces)
        {
            return (
                Math.Round(InchesToMillimeter(minX), decimalPlaces),
                Math.Round(InchesToMillimeter(maxX), decimalPlaces),
                Math.Round(InchesToMillimeter(minY), decimalPlaces),
                Math.Round(InchesToMillimeter(maxY), decimalPlaces),
                Math.Round(InchesToMillimeter(minZ), decimalPlaces),
                Math.Round(InchesToMillimeter(maxZ), decimalPlaces),
                Math.Round(VolumeConverter.ConvertIn3ToCm3(volume), decimalPlaces),
                Math.Round(VolumeConverter.ConvertFt3ToM3(volumeScale), decimalPlaces)
            );
        }

        /// <summary>
        /// Validates a single axis range (min/max values).
        /// Checks that min is less than max, min is <= 0, and max is >= 0.
        /// </summary>
        /// <param name="minValue">Minimum value for the axis</param>
        /// <param name="maxValue">Maximum value for the axis</param>
        /// <param name="axisName">Name of the axis (X, Y, or Z) for error messages</param>
        /// <returns>Tuple containing: validation result, list of error messages if any</returns>
        public static (bool IsValid, List<string> Errors) ValidateAxisRange(
            double minValue, double maxValue, string axisName)
        {
            var errors = new List<string>();

            if (minValue > 0)
                errors.Add($"Min {axisName} <= 0");
            if (maxValue < 0)
                errors.Add($"Max {axisName} >= 0");
            if (minValue >= maxValue)
            {
                errors.Add($"Min {axisName} < Max {axisName}");
                errors.Add($"Max {axisName} > Min {axisName}");
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Validates all three axes (X, Y, Z) simultaneously.
        /// Aggregates validation results and error messages for each axis.
        /// </summary>
        /// <param name="minX">Minimum X value</param>
        /// <param name="maxX">Maximum X value</param>
        /// <param name="minY">Minimum Y value</param>
        /// <param name="maxY">Maximum Y value</param>
        /// <param name="minZ">Minimum Z value</param>
        /// <param name="maxZ">Maximum Z value</param>
        /// <returns>Tuple containing: overall validation result, dictionary of axis names to error lists</returns>
        public static (bool AllValid, Dictionary<string, List<string>> AxisErrors) ValidateAllAxes(
            double minX, double maxX,
            double minY, double maxY,
            double minZ, double maxZ)
        {
            var axisErrors = new Dictionary<string, List<string>>();

            var (xValid, xErrors) = ValidateAxisRange(minX, maxX, "X");
            var (yValid, yErrors) = ValidateAxisRange(minY, maxY, "Y");
            var (zValid, zErrors) = ValidateAxisRange(minZ, maxZ, "Z");

            axisErrors["X"] = xErrors;
            axisErrors["Y"] = yErrors;
            axisErrors["Z"] = zErrors;

            return (xValid && yValid && zValid, axisErrors);
        }

        /// <summary>
        /// Generates the SCAD method call string and include directive for a created axis.
        /// Formats the module name into a callable method and include statement.
        /// </summary>
        /// <param name="moduleName">The name of the axis module</param>
        /// <param name="callingMethodLength">Length of the calling method (used for string operations)</param>
        /// <returns>Tuple containing: calling method string (e.g., "Get_module_name();"), include directive string</returns>
        public static (string CallingMethod, string IncludeFile) GenerateAxisMethodCall(
            string moduleName, int callingMethodLength)
        {
            var callingMethod = $"Get_{moduleName}();";
            var includeFile = $"include <{callingMethod.ToLower().Replace("();", "")}.scad>";

            return (callingMethod, includeFile);
        }

        /// <summary>
        /// Validation result data structure for axis range validation.
        /// Contains error information for each axis without ViewModel coupling.
        /// </summary>
        public class AxisValidationResult
        {
            /// <summary>
            /// Overall validation status - true if all axes are valid.
            /// </summary>
            public bool IsValid { get; set; }

            /// <summary>
            /// Individual axis validation results.
            /// </summary>
            public bool XAxisValid { get; set; }
            public bool YAxisValid { get; set; }
            public bool ZAxisValid { get; set; }

            /// <summary>
            /// Error messages keyed by property name (MinXValue, MaxXValue, etc.)
            /// </summary>
            public Dictionary<string, string> ErrorMessages { get; set; } = new();
        }

        /// <summary>
        /// Validates all three axes (X, Y, Z) and returns comprehensive validation results.
        /// Checks min/max relationships for each axis without modifying ViewModel state.
        /// Returns detailed error messages that the ViewModel can apply to its error collection.
        /// </summary>
        /// <param name="minX">Minimum X value</param>
        /// <param name="maxX">Maximum X value</param>
        /// <param name="minY">Minimum Y value</param>
        /// <param name="maxY">Maximum Y value</param>
        /// <param name="minZ">Minimum Z value</param>
        /// <param name="maxZ">Maximum Z value</param>
        /// <returns>AxisValidationResult containing validation status and error messages</returns>
        public static AxisValidationResult ValidateAxisRanges(
            double minX, double maxX,
            double minY, double maxY,
            double minZ, double maxZ)
        {
            var result = new AxisValidationResult
            {
                ErrorMessages = new Dictionary<string, string>()
            };

            bool xValid = true;
            bool yValid = true;
            bool zValid = true;

            // X Axis Validation
            if (minX > 0)
            {
                result.ErrorMessages[nameof(minX)] = "Min X <= 0";
                xValid = false;
            }
            if (maxX < 0)
            {
                result.ErrorMessages[nameof(maxX)] = "Max X >= 0";
                xValid = false;
            }
            if (minX >= maxX)
            {
                result.ErrorMessages[$"{nameof(minX)}_range"] = "Min X < Max X";
                result.ErrorMessages[$"{nameof(maxX)}_range"] = "Max X > Min X";
                xValid = false;
            }

            // Y Axis Validation
            if (minY > 0)
            {
                result.ErrorMessages[nameof(minY)] = "Min Y <= 0";
                yValid = false;
            }
            if (maxY < 0)
            {
                result.ErrorMessages[nameof(maxY)] = "Max Y >= 0";
                yValid = false;
            }
            if (minY >= maxY)
            {
                result.ErrorMessages[$"{nameof(minY)}_range"] = "Min Y < Max Y";
                result.ErrorMessages[$"{nameof(maxY)}_range"] = "Max Y > Min Y";
                yValid = false;
            }

            // Z Axis Validation
            if (minZ > 0)
            {
                result.ErrorMessages[nameof(minZ)] = "Min Z <= 0";
                zValid = false;
            }
            if (maxZ < 0)
            {
                result.ErrorMessages[nameof(maxZ)] = "Max Z >= 0";
                zValid = false;
            }
            if (minZ >= maxZ)
            {
                result.ErrorMessages[$"{nameof(minZ)}_range"] = "Min Z < Max Z";
                result.ErrorMessages[$"{nameof(maxZ)}_range"] = "Max Z > Min Z";
                zValid = false;
            }

            result.XAxisValid = xValid;
            result.YAxisValid = yValid;
            result.ZAxisValid = zValid;
            result.IsValid = xValid && yValid && zValid;

            return result;
        }

        /// <summary>
        /// Callback delegate for custom axis creation completion.
        /// Called when axis generation completes with all updated values.
        /// </summary>
        public delegate Task CreateCustomAxisCallbackAsync(
            CustomAxis customAxis,
            double minXValue,
            double maxXValue,
            double minYValue,
            double maxYValue,
            double minZValue,
            double maxZValue,
            int callingMethodLength,
            bool unitHasChanged);

        /// <summary>
        /// Creates a custom axis with the specified settings and invokes a callback with results.
        /// Handles input validation, unit conversions, backend axis generation, and output value formatting.
        /// </summary>
        /// <param name="minX">Minimum X value in the selected unit system</param>
        /// <param name="maxX">Maximum X value in the selected unit system</param>
        /// <param name="minY">Minimum Y value in the selected unit system</param>
        /// <param name="maxY">Maximum Y value in the selected unit system</param>
        /// <param name="minZ">Minimum Z value in the selected unit system</param>
        /// <param name="maxZ">Maximum Z value in the selected unit system</param>
        /// <param name="selectedUnit">Current unit system (Metric or Imperial)</param>
        /// <param name="selectedBackground">Background theme for the axis</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <param name="unitHasChanged">Whether the unit system was just changed</param>
        /// <param name="onAxisCreated">Callback invoked with generated axis and converted values</param>
        /// <returns>Task representing the async axis generation operation</returns>
        public static async Task CreateCustomAxisWithCallbackAsync(
            double minX, double maxX,
            double minY, double maxY,
            double minZ, double maxZ,
            UnitSystem selectedUnit,
            BackgroundType selectedBackground,
            int decimalPlaces,
            bool unitHasChanged,
            CreateCustomAxisCallbackAsync onAxisCreated)
        {
            // Validate input values are numeric
            if (double.IsNaN(minX) || double.IsNaN(maxX) ||
                double.IsNaN(minY) || double.IsNaN(maxY) ||
                double.IsNaN(minZ) || double.IsNaN(maxZ))
            {
                // Invalid input - return early, ViewModel will show error message
                return;
            }

            try
            {
                // Convert input values from imperial to metric if needed
                double convertedMinX, convertedMaxX, convertedMinY, convertedMaxY, convertedMinZ, convertedMaxZ;

                if (selectedUnit == UnitSystem.Imperial && !unitHasChanged)
                {
                    // Convert if inputs are inches but backend expects mm
                    convertedMinX = Math.Round(InchesToMillimeter(minX), decimalPlaces);
                    convertedMaxX = Math.Round(InchesToMillimeter(maxX), decimalPlaces);
                    convertedMinY = Math.Round(InchesToMillimeter(minY), decimalPlaces);
                    convertedMaxY = Math.Round(InchesToMillimeter(maxY), decimalPlaces);
                    convertedMinZ = Math.Round(InchesToMillimeter(minZ), decimalPlaces);
                    convertedMaxZ = Math.Round(InchesToMillimeter(maxZ), decimalPlaces);
                }
                else
                {
                    convertedMinX = minX;
                    convertedMaxX = maxX;
                    convertedMinY = minY;
                    convertedMaxY = maxY;
                    convertedMinZ = minZ;
                    convertedMaxZ = maxZ;
                }

                // Build axis settings for backend
                var axisSettings = new AxisSettings(
                    outputDirectory: "",
                    backgroundType: selectedBackground,
                    measureType: selectedUnit,
                    minX: convertedMinX,
                    maxX: convertedMaxX,
                    minY: convertedMinY,
                    maxY: convertedMaxY,
                    minZ: convertedMinZ,
                    maxZ: convertedMaxZ);

                // Call backend to generate axis
                var customAxis = await GUI.SetAxis(axisSettings);

                if (customAxis == null)
                    return;

                // Convert generated values back to display format
                bool newUnitHasChanged = false;
                double displayMinX, displayMaxX, displayMinY, displayMaxY, displayMinZ, displayMaxZ;

                if (selectedUnit == UnitSystem.Imperial)
                {
                    // Convert from mm back to inches for display
                    displayMinX = Math.Round(MillimeterToInches(customAxis.Settings.MinX), decimalPlaces);
                    displayMaxX = Math.Round(MillimeterToInches(customAxis.Settings.MaxX), decimalPlaces);
                    displayMinY = Math.Round(MillimeterToInches(customAxis.Settings.MinY), decimalPlaces);
                    displayMaxY = Math.Round(MillimeterToInches(customAxis.Settings.MaxY), decimalPlaces);
                    displayMinZ = Math.Round(MillimeterToInches(customAxis.Settings.MinZ), decimalPlaces);
                    displayMaxZ = Math.Round(MillimeterToInches(customAxis.Settings.MaxZ), decimalPlaces);
                }
                else
                {
                    // Display metric values as-is
                    displayMinX = Math.Round(customAxis.Settings.MinX, decimalPlaces);
                    displayMaxX = Math.Round(customAxis.Settings.MaxX, decimalPlaces);
                    displayMinY = Math.Round(customAxis.Settings.MinY, decimalPlaces);
                    displayMaxY = Math.Round(customAxis.Settings.MaxY, decimalPlaces);
                    displayMinZ = Math.Round(customAxis.Settings.MinZ, decimalPlaces);
                    displayMaxZ = Math.Round(customAxis.Settings.MaxZ, decimalPlaces);
                }

                // Calculate calling method length for UI display
                int callingMethodLength = customAxis.CallingMethod.Length - 1;

                // Invoke callback with all updated values
                await onAxisCreated(
                    customAxis,
                    displayMinX,
                    displayMaxX,
                    displayMinY,
                    displayMaxY,
                    displayMinZ,
                    displayMaxZ,
                    callingMethodLength,
                    newUnitHasChanged);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating custom axis: {ex.Message}");
            }
        }

        /// <summary>
        /// Data structure for axis input conversion results.
        /// Contains all converted axis range values without ViewModel coupling.
        /// </summary>
        public class AxisConversionResult
        {
            /// <summary>
            /// Converted axis range values in target unit system.
            /// </summary>
            public double MinX { get; set; }
            public double MaxX { get; set; }
            public double MinY { get; set; }
            public double MaxY { get; set; }
            public double MinZ { get; set; }
            public double MaxZ { get; set; }
            public double Volume { get; set; }
            public double VolumeScale { get; set; }
        }

        /// <summary>
        /// Converts axis input values and volume from metric to imperial units.
        /// Pure calculation function with no side effects or ViewModel coupling.
        /// </summary>
        /// <param name="minX">Minimum X value in millimeters</param>
        /// <param name="maxX">Maximum X value in millimeters</param>
        /// <param name="minY">Minimum Y value in millimeters</param>
        /// <param name="maxY">Maximum Y value in millimeters</param>
        /// <param name="minZ">Minimum Z value in millimeters</param>
        /// <param name="maxZ">Maximum Z value in millimeters</param>
        /// <param name="volume">Volume in cubic centimeters</param>
        /// <param name="volumeScale">Volume scale in cubic meters</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <returns>AxisConversionResult with values converted to inches</returns>
        public static AxisConversionResult ConvertInputToImperial(
            double minX, double maxX,
            double minY, double maxY,
            double minZ, double maxZ,
            double volume, double volumeScale,
            int decimalPlaces)
        {
            return new AxisConversionResult
            {
                MinX = Math.Round(MillimeterToInches(minX), decimalPlaces),
                MaxX = Math.Round(MillimeterToInches(maxX), decimalPlaces),
                MinY = Math.Round(MillimeterToInches(minY), decimalPlaces),
                MaxY = Math.Round(MillimeterToInches(maxY), decimalPlaces),
                MinZ = Math.Round(MillimeterToInches(minZ), decimalPlaces),
                MaxZ = Math.Round(MillimeterToInches(maxZ), decimalPlaces),
                Volume = Math.Round(VolumeConverter.ConvertCm3ToIn3(volume), decimalPlaces),
                VolumeScale = Math.Round(VolumeConverter.ConvertM3ToFt3(volumeScale), decimalPlaces)
            };
        }

        /// <summary>
        /// Converts axis input values and volume from imperial to metric units.
        /// Pure calculation function with no side effects or ViewModel coupling.
        /// </summary>
        /// <param name="minX">Minimum X value in inches</param>
        /// <param name="maxX">Maximum X value in inches</param>
        /// <param name="minY">Minimum Y value in inches</param>
        /// <param name="maxY">Maximum Y value in inches</param>
        /// <param name="minZ">Minimum Z value in inches</param>
        /// <param name="maxZ">Maximum Z value in inches</param>
        /// <param name="volume">Volume in cubic inches</param>
        /// <param name="volumeScale">Volume scale in cubic feet</param>
        /// <param name="decimalPlaces">Number of decimal places for rounding</param>
        /// <returns>AxisConversionResult with values converted to millimeters</returns>
        public static AxisConversionResult ConvertInputToMetric(
            double minX, double maxX,
            double minY, double maxY,
            double minZ, double maxZ,
            double volume, double volumeScale,
            int decimalPlaces)
        {
            return new AxisConversionResult
            {
                MinX = Math.Round(InchesToMillimeter(minX), decimalPlaces),
                MaxX = Math.Round(InchesToMillimeter(maxX), decimalPlaces),
                MinY = Math.Round(InchesToMillimeter(minY), decimalPlaces),
                MaxY = Math.Round(InchesToMillimeter(maxY), decimalPlaces),
                MinZ = Math.Round(InchesToMillimeter(minZ), decimalPlaces),
                MaxZ = Math.Round(InchesToMillimeter(maxZ), decimalPlaces),
                Volume = Math.Round(VolumeConverter.ConvertIn3ToCm3(volume), decimalPlaces),
                VolumeScale = Math.Round(VolumeConverter.ConvertFt3ToM3(volumeScale), decimalPlaces)
            };
        }
    }
}
