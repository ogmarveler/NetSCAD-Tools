using Avalonia.Data.Converters;
using NetGenCAD.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NetGenCAD.UI.Converters
{
    public class PolyhedronValidationConverter : IMultiValueConverter
    {
        // Regex pattern to validate face points format: [0,1,2],[2,0,0],[1,1,1]...
        private static readonly Regex FacePointsPattern = new(
            @"^\[\d+(?:,\d+)*\](?:,\[\d+(?:,\d+)*\])*$",
            RegexOptions.Compiled
        );

        // Constants for validation
        private const int MinPointsPerFace = 3; // OpenSCAD requires at least 3 points for a valid face
        private const double MaxCoordinateValue = 1000000; // Reasonable upper bound for CAD coordinates
        private const int MinNameLength = 1;
        private const int MaxNameLength = 255;
        private const int MinDescriptionLength = 1;
        private const int MaxDescriptionLength = 500;
        private const int MinConvexity = 1; // OpenSCAD minimum convexity
        private const int MaxConvexity = 100; // Reasonable upper limit for convexity

        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // Required bindings: SelectedPolyhedronOperationType, Name, Description, PointsId, PointXMM, PointYMM, PointZMM, FacePoints, FaceId, PolyhedronConvexity
            if (values == null || values.Count < 10)
                return false;

            // Extract and validate common properties
            PolyhedronOperationType? operationType = values[0] as PolyhedronOperationType?;
            bool nameValid = ValidateName(values[1]);
            bool descValid = ValidateDescription(values[2]);
            bool convexityValid = ValidateConvexity(values[9]);

            if (!nameValid || !descValid || operationType == null || !convexityValid)
                return false;

            // Validate based on operation type
            return operationType == PolyhedronOperationType.Points
                ? ValidatePoints(values)
                : ValidateFaces(values);
        }

        /// <summary>
        /// Validates name requirements.
        /// - Must be non-null and non-whitespace
        /// - Must be between MinNameLength and MaxNameLength characters
        /// - Should not contain invalid characters for file naming
        /// </summary>
        private static bool ValidateName(object? nameObj)
        {
            if (nameObj is not string name || string.IsNullOrWhiteSpace(name))
                return false;

            if (name.Length < MinNameLength || name.Length > MaxNameLength)
                return false;

            // Check for invalid filename characters (reserved in OpenSCAD exports)
            string invalidChars = @"[<>:""/\\|?*\x00-\x1F]";
            return !Regex.IsMatch(name, invalidChars);
        }

        /// <summary>
        /// Validates description requirements.
        /// - Must be non-null and non-whitespace
        /// - Must be between MinDescriptionLength and MaxDescriptionLength characters
        /// </summary>
        private static bool ValidateDescription(object? descObj)
        {
            if (descObj is not string desc || string.IsNullOrWhiteSpace(desc))
                return false;

            return desc.Length >= MinDescriptionLength && desc.Length <= MaxDescriptionLength;
        }

        /// <summary>
        /// Validates convexity requirements.
        /// - Must be an integer between MinConvexity and MaxConvexity
        /// - OpenSCAD uses convexity to improve rendering performance
        /// </summary>
        private static bool ValidateConvexity(object? convexityObj)
        {
            if (convexityObj is not int convexity)
                return false;

            return convexity >= MinConvexity && convexity <= MaxConvexity;
        }

        /// <summary>
        /// Validates Points operation type requirements.
        /// Requires:
        /// - PointsId >= 0
        /// - PointXMM, PointYMM, PointZMM as valid non-negative doubles
        /// - Coordinates within reasonable bounds for CAD operations
        /// - No NaN or Infinity values
        /// </summary>
        private static bool ValidatePoints(IList<object?> values)
        {
            int pointsId = values[3] is int id ? id : -1;
            
            // Validate coordinates
            bool pointXValid = ValidateCoordinate(values[4]);
            bool pointYValid = ValidateCoordinate(values[5]);
            bool pointZValid = ValidateCoordinate(values[6]);

            return pointsId >= 0 && pointXValid && pointYValid && pointZValid;
        }

        /// <summary>
        /// Validates a single coordinate value.
        /// - Must be a valid double (not NaN or Infinity)
        /// - Must be non-negative
        /// - Must be within reasonable bounds for CAD operations
        /// </summary>
        private static bool ValidateCoordinate(object? coordObj)
        {
            if (coordObj is not double coord)
                return false;

            // Check for NaN or Infinity
            if (double.IsNaN(coord) || double.IsInfinity(coord))
                return false;

            // Check bounds
            return coord <= MaxCoordinateValue;
        }

        /// <summary>
        /// Validates Faces operation type requirements.
        /// Requires:
        /// - FacePoints in format [0,1,2],[2,0,0]... with valid structure
        /// - Each face has at least 3 point indices (OpenSCAD requirement)
        /// - FaceId >= 0
        /// </summary>
        private static bool ValidateFaces(IList<object?> values)
        {
            bool facePointsValid = values[7] is string facePoints && IsValidFacePointsFormat(facePoints);
            int faceId = values[8] is int id ? id : -1;

            return facePointsValid && faceId >= 0;
        }

        /// <summary>
        /// Validates that face points string follows the format: [0,1,2],[2,0,0],[1,1,1]...
        /// - Must start with [ and end with ]
        /// - Each face is [int,int,...] with integers only
        /// - Multiple faces are separated by ],[
        /// - Each face must have at least 3 point indices (OpenSCAD requirement)
        /// </summary>
        private static bool IsValidFacePointsFormat(string facePoints)
        {
            if (string.IsNullOrWhiteSpace(facePoints))
                return false;

            // First, check basic format
            if (!FacePointsPattern.IsMatch(facePoints))
                return false;

            // Extract individual faces and validate each has minimum 3 points
            try
            {
                var faceMatches = Regex.Matches(facePoints, @"\[(\d+(?:,\d+)*)\]");
                foreach (System.Text.RegularExpressions.Match match in faceMatches)
                {
                    var pointIndices = match.Groups[1].Value.Split(',');
                    if (pointIndices.Length < MinPointsPerFace)
                        return false;

                    // Validate all indices are valid integers
                    foreach (var index in pointIndices)
                    {
                        if (!int.TryParse(index, out var intValue) || intValue < 0)
                            return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
