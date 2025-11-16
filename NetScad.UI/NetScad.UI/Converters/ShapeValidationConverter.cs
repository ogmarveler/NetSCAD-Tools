using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NetScad.UI.Converters
{
    public class ShapeValidationConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count < 12)
                return false;

            // Order: IsCubeSelected, IsRoundCubeSelected, IsCylinderSelected, LengthMM, WidthMM, HeightMM, RadiusMM, CylinderHeightMM, Name, Description
            bool isCube = values[0] is bool cube && cube;
            bool isRoundCube = values[1] is bool roundCube && roundCube;
            bool isCylinder = values[2] is bool cylinder && cylinder;
            bool isSurface = values[12] is bool surface && surface;
            bool isRoundSurface = values[13] is bool roundSurface && roundSurface;

            // Validate Name and Description (always required)
            bool nameValid = values[10] is string name && !string.IsNullOrWhiteSpace(name);
            bool descValid = values[11] is string desc && !string.IsNullOrWhiteSpace(desc);

            if (!nameValid || !descValid)
                return false;

            if (isCube || isRoundCube)
            {
                // Validate Length, Width, Height for Cube
                bool lengthValid = values[3] is double length && length > 0;
                bool widthValid = values[4] is double width && width > 0;
                bool heightValid = values[5] is double height && height > 0;
                
                return lengthValid && widthValid && heightValid;
            }
            else if (isCylinder)
            {
                // Validate Radius and CylinderHeight for Cylinder
                bool radiusValid = values[6] is double radius && radius > 0;
                bool radius1Valid = values[7] is double radius1 && radius1 > 0;
                bool radius2Valid = values[8] is double radius2 && radius2 > 0;
                bool cylHeightValid = values[9] is double cylHeight && cylHeight > 0;
                
                if (radius1Valid && radius2Valid)
                    return radius1Valid && radius2Valid && cylHeightValid;
                else
                    return radiusValid && cylHeightValid;
            }
            else if (isSurface || isRoundSurface)
            {
                bool lengthValid = values[3] is double length && length > 0;
                bool widthValid = values[4] is double width && width > 0;
                bool heightValid = values[5] is double height && height > 0;
                bool isSurfaceFilePath = values[14] is string surfaceFilePath && !string.IsNullOrWhiteSpace(surfaceFilePath);
                bool surfaceScaleXValid = values[15] is double surfaceScaleX && surfaceScaleX > 0;
                bool surfaceScaleYValid = values[16] is double surfaceScaleY && surfaceScaleY > 0;
                bool surfaceScaleZValid = values[17] is double surfaceScaleZ && surfaceScaleZ > 0;

                return lengthValid && widthValid && heightValid && isSurfaceFilePath && surfaceScaleXValid && surfaceScaleYValid && surfaceScaleZValid;
            }
                return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
