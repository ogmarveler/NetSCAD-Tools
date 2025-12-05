using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NetGenCAD.UI.Converters
{
    public class ShapeValidationConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count < 19)
                return false;

            // Order: IsCubeSelected, IsRoundCubeSelected, IsCylinderSelected, IsSphereSelected, LengthMM, WidthMM, HeightMM, RadiusMM, Radius1MM, Radius2MM, CylinderHeightMM, Name, Description, IsSurfaceSelected, IsRoundSurfaceSelected, SurfaceFilePath, SurfaceScaleX, SurfaceScaleY, SurfaceScaleZ
            bool isCube = values[0] is bool cube && cube;
            bool isRoundCube = values[1] is bool roundCube && roundCube;
            bool isCylinder = values[2] is bool cylinder && cylinder;
            bool isSphere = values[3] is bool sphere && sphere;
            bool isSurface = values[13] is bool surface && surface;
            bool isRoundSurface = values[14] is bool roundSurface && roundSurface;

            // Validate Name and Description (always required)
            bool nameValid = values[11] is string name && !string.IsNullOrWhiteSpace(name);
            bool descValid = values[12] is string desc && !string.IsNullOrWhiteSpace(desc);

            if (!nameValid || !descValid)
                return false;

            if (isCube || isRoundCube)
            {
                // Validate Length, Width, Height for Cube
                bool lengthValid = values[4] is double length && length > 0;
                bool widthValid = values[5] is double width && width > 0;
                bool heightValid = values[6] is double height && height > 0;

                return lengthValid && widthValid && heightValid;
            }
            else if (isCylinder)
            {
                // Validate Radius and CylinderHeight for Cylinder
                bool radiusValid = values[7] is double radius && radius > 0;
                bool radius1Valid = values[8] is double radius1 && radius1 > 0;
                bool radius2Valid = values[9] is double radius2 && radius2 > 0;
                bool cylHeightValid = values[10] is double cylHeight && cylHeight > 0;

                if (radius1Valid && radius2Valid)
                    return radius1Valid && radius2Valid && cylHeightValid;
                else
                    return radiusValid && cylHeightValid;
            }
            else if (isSphere)
            {
                // Validate Radius for Sphere
                bool radiusValid = values[7] is double radius && radius > 0;
                return radiusValid;
            }
            else if (isSurface || isRoundSurface)
            {
                bool lengthValid = values[4] is double length && length > 0;
                bool widthValid = values[5] is double width && width > 0;
                bool heightValid = values[6] is double height && height > 0;
                bool isSurfaceFilePath = values[15] is string surfaceFilePath && !string.IsNullOrWhiteSpace(surfaceFilePath);
                bool surfaceScaleXValid = values[16] is double surfaceScaleX && surfaceScaleX > 0;
                bool surfaceScaleYValid = values[17] is double surfaceScaleY && surfaceScaleY > 0;
                bool surfaceScaleZValid = values[18] is double surfaceScaleZ && surfaceScaleZ > 0;

                return lengthValid && widthValid && heightValid && isSurfaceFilePath && surfaceScaleXValid && surfaceScaleYValid && surfaceScaleZValid;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
