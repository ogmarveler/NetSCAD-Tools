using NetScad.Core.Measurements;
using NetScad.Core.Utility;
using System.Text;
using static NetScad.Axis.SCAD.Utility.AxisConfig;
using static NetScad.Axis.SCAD.Utility.BlockStatement;
using static NetScad.Core.Measurements.Selector;
using static NetScad.Core.Measurements.Conversion;
using NetScad.Core.Primitives;

namespace NetScad.Axis.SCAD.Modules
{
    public class Axis
    {
        public static AxisSettings CheckAxisSettings(AxisSettings axSet)
        {
            if (axSet is not null)
            {
                // X Axis
                // Default to 0-300mm if range is zero
                if (Math.Abs(axSet.MinX - axSet.MaxX) == 0) { axSet.MinX = 0; axSet.MaxX = 300; }

                // Use range if min and max are above or below 0 axis points - move to 0 axis point
                else if (axSet.MinX > 0 && axSet.MaxX > 0)
                {
                    axSet.MaxX = Math.Abs(axSet.MaxX - axSet.MinX);
                    axSet.MinX = 0;
                }
                else if (axSet.MinX < 0 && axSet.MaxX < 0)
                {
                    axSet.MinX = Math.Abs(axSet.MaxX - axSet.MinX) * -1;
                    axSet.MaxX = 0;
                }
                // Use range if min and max inverted default to min and max
                else if (axSet.MaxX < axSet.MinX)
                {
                    if (axSet.MaxX < 0) // MaxX = 0, MinX --> -X
                    {
                        axSet.MinX = Math.Abs(axSet.MaxX - axSet.MinX) * -1;
                        axSet.MaxX = 0;
                    }
                    else // MinX = 0, MaxX --> +X
                    {
                        axSet.MaxX = Math.Abs(axSet.MaxX - axSet.MinX);
                        axSet.MinX = 0;
                    }
                }

                // Y Axis
                // Default to 0-300mm if range is zero
                if (Math.Abs(axSet.MinY - axSet.MaxY) == 0)
                {
                    axSet.MinY = 0;
                    axSet.MaxY = 300;
                }
                // Use range if min and max are above or below 0 axis points - move to 0 axis point
                else if (axSet.MinY > 0 && axSet.MaxY > 0)
                {
                    axSet.MaxY = Math.Abs(axSet.MaxY - axSet.MinY);
                    axSet.MinY = 0;
                }
                else if (axSet.MinY < 0 && axSet.MaxY < 0)
                {
                    axSet.MinY = Math.Abs(axSet.MaxY - axSet.MinY) * -1;
                    axSet.MaxY = 0;
                }
                // Use range if min and max inverted default to min and max
                else if (axSet.MaxY < axSet.MinY)
                {
                    if (axSet.MaxY < 0) // MaxY = 0, MinY --> -Y
                    {
                        axSet.MinY = Math.Abs(axSet.MaxY - axSet.MinY) * -1;
                        axSet.MaxY = 0;
                    }
                    else // MinY = 0, MaxY --> +Y
                    {
                        axSet.MaxY = Math.Abs(axSet.MaxY - axSet.MinY);
                        axSet.MinY = 0;
                    }
                }

                // Z Axis
                // Default to 0-300mm if range is zero
                if (Math.Abs(axSet.MinZ - axSet.MaxZ) == 0)
                {
                    axSet.MinZ = 0;
                    axSet.MaxZ = 300;
                }
                // Use range if min and max are above or below 0 axis points - move to 0 axis point
                else if (axSet.MinZ > 0 && axSet.MaxZ > 0)
                {
                    axSet.MaxZ = Math.Abs(axSet.MaxZ - axSet.MinZ);
                    axSet.MinZ = 0;
                }
                else if (axSet.MinZ < 0 && axSet.MaxZ < 0)
                {
                    axSet.MinZ = Math.Abs(axSet.MaxZ - axSet.MinZ) * -1;
                    axSet.MaxZ = 0;
                }
                // Use range if min and max inverted default to min and max
                else if (axSet.MaxZ < axSet.MinZ)
                {
                    if (axSet.MaxZ < 0) // MaxZ = 0, MinZ --> -Z
                    {
                        axSet.MinZ = Math.Abs(axSet.MaxZ - axSet.MinZ) * -1;
                        axSet.MaxZ = 0;
                    }
                    else // MinZ = 0, MaxZ --> +Z
                    {
                        axSet.MaxZ = Math.Abs(axSet.MaxZ - axSet.MinZ);
                        axSet.MinZ = 0;
                    }
                }

                // For Metric, Axis will set measurements to 20mm, 10mm, 5mm, 1mm increments.
                // For Imperial, Axis will be set to 1/4, 1/8, 1/16, and 1/32 inch increments.
                // For larger measurements, adjust Min, Max, and Scale accordingly to keep axis readable.
                var precision = axSet.UnitSystem == UnitSystem.Imperial ? FractionalInch.Inch4.ToMm(1) : 1;
                axSet.MinX = AdjustCoordinate(coordinate: axSet.MinX, increment: axSet.IncrementX, precision: precision);
                axSet.MaxX = AdjustCoordinate(coordinate: axSet.MaxX, increment: axSet.IncrementX, precision: precision);
                axSet.MinY = AdjustCoordinate(coordinate: axSet.MinY, increment: axSet.IncrementY, precision: precision);
                axSet.MaxY = AdjustCoordinate(coordinate: axSet.MaxY, increment: axSet.IncrementY, precision: precision);
                axSet.MinZ = AdjustCoordinate(coordinate: axSet.MinZ, increment: axSet.IncrementZ, precision: precision);
                axSet.MaxZ = AdjustCoordinate(coordinate: axSet.MaxZ, increment: axSet.IncrementZ, precision: precision);
            }
            else { axSet = new AxisSettings(outputDirectory: PathHelper.GetProjectRoot()); }
            return axSet;
        }

        public static CustomAxis ConfigureAxisModule(AxisSettings axSet)
        {
            // Object to hold the module, color, and labels
            // Create unique module name based on settings
            var xAxis = Math.Abs(axSet.MaxX - axSet.MinX);
            var yAxis = Math.Abs(axSet.MaxY - axSet.MinY);
            var zAxis = Math.Abs(axSet.MaxZ - axSet.MinZ);

            // Labels based on measurement type and total cubic size - mm to inches or mm
            var xLabel = axSet.UnitSystem == UnitSystem.Imperial ? $"{Math.Round(MillimeterToInches(xAxis), 0)}" : $"{xAxis}";
            var yLabel = axSet.UnitSystem == UnitSystem.Imperial ? $"{Math.Round(MillimeterToInches(yAxis), 0)}" : $"{yAxis}";
            var zLabel = axSet.UnitSystem == UnitSystem.Imperial ? $"{Math.Round(MillimeterToInches(zAxis), 0)}" : $"{zAxis}";

            // Total cubic volume of the workspace - mm to inches or mm to cm
            var xMsr = axSet.UnitSystem == UnitSystem.Imperial ? MillimeterToInches(xAxis) : MillimeterToCentimeter(xAxis);
            var yMsr = axSet.UnitSystem == UnitSystem.Imperial ? MillimeterToInches(yAxis) : MillimeterToCentimeter(yAxis);
            var zMsr = axSet.UnitSystem == UnitSystem.Imperial ? MillimeterToInches(zAxis) : MillimeterToCentimeter(zAxis);
            var tVolume = xMsr * yMsr * zMsr; // inches^3 or cm^3

            // Measure in cubic feet or cubic meters - mm to feet or mm to meter
            var xMsrScale = axSet.UnitSystem == UnitSystem.Imperial ? MillimeterToFeet(xAxis) : MillimeterToMeter(xAxis);
            var yMsrScale = axSet.UnitSystem == UnitSystem.Imperial ? MillimeterToFeet(yAxis) : MillimeterToMeter(yAxis);
            var zMsrScale = axSet.UnitSystem == UnitSystem.Imperial ? MillimeterToFeet(zAxis) : MillimeterToMeter(zAxis);
            var tVolumeScale = xMsrScale * yMsrScale * zMsrScale; // ft^3 or mtr^3

            // Labels based on measurement type and axis start point
            var xStart = axSet.UnitSystem == UnitSystem.Imperial ? Math.Round(MillimeterToInches(axSet.MinX), 0) : axSet.MinX;
            var yStart = axSet.UnitSystem == UnitSystem.Imperial ? Math.Round(MillimeterToInches(axSet.MinY), 0) : axSet.MinY;
            var zStart = axSet.UnitSystem == UnitSystem.Imperial ? Math.Round(MillimeterToInches(axSet.MinZ), 0) : axSet.MinZ;
            var startLabel = $"_Orig_{xStart}x{yStart}x{zStart}".Replace("-", "N");

            // Create the Axis Module
            var axMod = new CustomAxis
            {
                // Add the total cubic volume measurements
                TotalCubicVolume = tVolume,
                TotalCubicVolumeScale = tVolumeScale
            };
            var unit = axSet.UnitSystem == UnitSystem.Imperial ? "in" : "mm";
            var unitLabel = axSet.UnitSystem == UnitSystem.Imperial ? "Inch" : "MM";
            var scale = axSet.UnitSystem == UnitSystem.Imperial ? Inch.Inch.ToMm(1) : 1;
            var axisColor = axSet.OpenScadColor.ToString().ToLower();

            // Labels for module names and call methods
            axMod.ModuleName = $"{axSet.BackgroundType}_{xLabel}x{yLabel}x{zLabel}_{unitLabel}";
            // For negative axis origins
            if (xStart == 0 && yStart == 0 && zStart == 0) { axMod.ModuleName += "_Orig_0x0x0"; }
            else { axMod.ModuleName += startLabel; }

            // Call Method in user's main SCAD file for axis retrieval from axis library
            axMod.CallingMethod = $"Get_{axMod.ModuleName}();";

            // Create Axis Module
            var sb = new StringBuilder();
            sb.AppendLine($"// {axMod.ModuleName} {axSet.UnitSystem} {AxisModuleFormats.ModuleComments}");
            sb.AppendLine($"module {axMod.ModuleName.ToLower()}(colorVal, alpha) {{");
            sb.AppendLine($"    color(colorVal, alpha) {{"); // Wrap all in color
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "x", range: [axSet.MinX, axSet.IncrementX, axSet.MaxX])}{{   {AxisModuleFormats.XOffsetMarker}   }}");  // X Axis Marker
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "y", range: [axSet.MinY, axSet.IncrementY, axSet.MaxY])}{{   {AxisModuleFormats.YOffsetMarker}   }}");  // Y Axis Marker
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "z", range: [axSet.MinZ, axSet.IncrementZ, axSet.MaxZ])}{{   {AxisModuleFormats.ZOffsetMarker}   }}");  // Z Axis Marker
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "x", range: [axSet.MinX, axSet.IncrementX2, axSet.MaxX])}{{   {AxisModuleFormats.XOffsetMarker2}   }}");  // X Axis Marker 2
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "y", range: [axSet.MinY, axSet.IncrementY2, axSet.MaxY])}{{   {AxisModuleFormats.YOffsetMarker2}   }}");  // Y Axis Marker 2
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "z", range: [axSet.MinZ, axSet.IncrementZ2, axSet.MaxZ])}{{   {AxisModuleFormats.ZOffsetMarker2}   }}");  // Z Axis Marker 2
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "x", range: [axSet.MinX, axSet.IncrementX3, axSet.MaxX])}{{   {AxisModuleFormats.XOffsetMarker3}   }}");  // X Axis Marker 3
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "y", range: [axSet.MinY, axSet.IncrementY3, axSet.MaxY])}{{   {AxisModuleFormats.YOffsetMarker3}   }}");  // Y Axis Marker 3
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "z", range: [axSet.MinZ, axSet.IncrementZ3, axSet.MaxZ])}{{   {AxisModuleFormats.ZOffsetMarker3}   }}");  // Z Axis Marker 3
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "x", range: [axSet.MinX, axSet.IncrementX4, axSet.MaxX])}{{   {AxisModuleFormats.XOffsetMarker4}   }}");  // X Axis Marker 4
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "y", range: [axSet.MinY, axSet.IncrementY4, axSet.MaxY])}{{   {AxisModuleFormats.YOffsetMarker4}   }}");  // Y Axis Marker 4
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "z", range: [axSet.MinZ, axSet.IncrementZ4, axSet.MaxZ])}{{   {AxisModuleFormats.ZOffsetMarker4}   }}");  // Z Axis Marker 4
            sb.AppendLine($"         // Axis Labels"); // Create the Axis Markers - main marker used for measurements, half and quarter markers for visual reference
            sb.AppendLine($"         unit = \"{unit}\";"); // Set unit for labels
            sb.AppendLine($"         scale = {scale};\n"); // Set scale for labels
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "i", range: [axSet.MinX, axSet.IncrementX, axSet.MaxX])}{{   {AxisModuleFormats.XOffsetLabel}   }}");  // X Axis Label
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "i", range: [axSet.MinY, axSet.IncrementY, axSet.MaxY])}{{   {AxisModuleFormats.YOffsetLabel}   }}");  // Y Axis Label
            sb.AppendLine($"         {GetIterationHeader(scope: OScadIteration.For, iterator: "i", range: [axSet.MinZ, axSet.IncrementZ, axSet.MaxZ])}{{   {AxisModuleFormats.ZOffsetLabel}   }}");  // Z Axis Label
            sb.AppendLine($"  }}");
            sb.AppendLine($"}}");
            sb.AppendLine($"// End of {axMod.ModuleName} Module");
            // Module contents
            axMod.AxisModule = sb.ToString();

            // Include the settings used to create the axis for reference
            axMod.Settings = axSet;
            return axMod;
        }
    }
}