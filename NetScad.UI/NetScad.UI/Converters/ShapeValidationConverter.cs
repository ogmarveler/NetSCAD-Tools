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
            if (values == null || values.Count < 7)
                return false;

            // Order: IsCubeSelected, IsCylinderSelected, LengthMM, WidthMM, HeightMM, RadiusMM, CylinderHeightMM, Name, Description
            bool isCube = values[0] is bool cube && cube;
            bool isCylinder = values[1] is bool cylinder && cylinder;
            
            // Validate Name and Description (always required)
            bool nameValid = values[7] is string name && !string.IsNullOrWhiteSpace(name);
            bool descValid = values[8] is string desc && !string.IsNullOrWhiteSpace(desc);
            
            if (!nameValid || !descValid)
                return false;

            if (isCube)
            {
                // Validate Length, Width, Height for Cube
                bool lengthValid = values[2] is double length && length > 0;
                bool widthValid = values[3] is double width && width > 0;
                bool heightValid = values[4] is double height && height > 0;
                
                return lengthValid && widthValid && heightValid;
            }
            else if (isCylinder)
            {
                // Validate Radius and CylinderHeight for Cylinder
                bool radiusValid = values[5] is double radius && radius > 0;
                bool cylHeightValid = values[6] is double cylHeight && cylHeight > 0;
                
                return radiusValid && cylHeightValid;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
