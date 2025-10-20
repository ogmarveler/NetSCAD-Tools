using System;
using static NetScad.Core.Measurements.Conversion;

namespace NetScad.Core.Measurements
{
    public static class VolumeCalculation
    {
        public static (double rangeX,double rangeY, double rangeZ, string volume) GetVolume(
            double minX,
            double maxX,
            double minY,
            double maxY,
            double minZ,
            double maxZ,
            string unit)
        {
            var xAxis = Math.Abs(maxX - minX);
            var yAxis = Math.Abs(maxY - minY);
            var zAxis = Math.Abs(maxZ - minZ);
            string? _volume;
            if (unit == "mm")
            {
                _volume = $"{MillimeterToCentimeter(xAxis) * MillimeterToCentimeter(yAxis) * MillimeterToCentimeter(zAxis)} cm³";
            }
            else
            {
                _volume = $"{xAxis * yAxis * zAxis} in³";
            }


                return (xAxis, yAxis, zAxis, _volume);
        }
    }
}
