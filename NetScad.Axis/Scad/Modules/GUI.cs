using NetScad.Core.Measurements;
using NetScad.Axis.SCAD.Models;
using NetScad.Core.Utility;
using static NetScad.Axis.SCAD.Modules.Axis;
using static NetScad.Axis.SCAD.Utility.AxisConfig;

namespace NetScad.Axis.SCAD.Modules
{
    public static class GUI
    {
        // OpenSCAD GUI axis - Saves into main axes file that can be called from your main SCAD project file
        public static async Task<CustomAxis> SetAxis(AxisSettings axSet, CancellationToken cancellationToken = default)
        {
            // Validate and configure axis settings
            // Stateless - always re-check settings
            var _axisSettings = CheckAxisSettings(axSet);
            var _customAxis = ConfigureAxisModule(_axisSettings);
            var _moduleNameLower = _customAxis.ModuleName.ToLower();

            // Write to file - import into output SCAD file
            var _axisFilePath = Path.Combine(_customAxis.Settings.OutputDirectory, $"{_moduleNameLower}.scad");

            // Since this is a code-driven module, always overwrite generated module since it regenerates in real-time
            await Output.WriteToSCAD(content: _customAxis.AxisModule, filePath: _axisFilePath, overWrite: true, cancellationToken: cancellationToken);
            var axisColor = _customAxis.Settings.OpenScadColor.ToString().ToLower();

            // Store in Axes reference file - Comments and Instructions
            var _moduleComment = $"// 3D Axis Module - {_customAxis.ModuleName.Replace("_", " ")}\n" +
                $"// Calling Method: Get_{_customAxis.CallingMethod}\n" +
                $"// Settings: UnitSystem={_customAxis.Settings.UnitSystem}, BackgroundType={_customAxis.Settings.BackgroundType}, MinX={_customAxis.Settings.MinX}, MaxX={_customAxis.Settings.MaxX}, MinY={_customAxis.Settings.MinY}, " +
                $"MaxY={_customAxis.Settings.MaxY}, MinZ={_customAxis.Settings.MinZ}, MaxZ={_customAxis.Settings.MaxZ}";

            // Store in Axes reference file - Module definition and contents
            var _scadFilePath = Path.Combine(_customAxis.Settings.OutputDirectory, $"axes.scad");
            var _importStatement = $"{_moduleComment}\n" +
                $"include <{_moduleNameLower}.scad>;\n" +
                $"module Get_{_customAxis.ModuleName}(colorVal = \"{(_customAxis.Settings.BackgroundType == Selector.BackgroundType.Light ? Colors.OpenScadColor.Black : Colors.OpenScadColor.White)}\", " +
                $"alpha = {axSet.AxisColorAlpha}) {{\n" +
                $"  {_moduleNameLower}(colorVal = colorVal, alpha = alpha);\n" +
                $"}}\n\n";
            await Output.AppendToSCAD(content: _importStatement, filePath: _scadFilePath, cancellationToken: cancellationToken);
            return _customAxis;
        }
    }
}