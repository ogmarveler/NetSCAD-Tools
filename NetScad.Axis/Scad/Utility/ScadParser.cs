using System.Collections.ObjectModel;
using static NetScad.Core.Measurements.Conversion;
using static NetScad.Core.Measurements.Selector;
using static NetScad.Core.Measurements.VolumeCalculation;
using NetScad.Axis.Scad.Models;

namespace NetScad.Axis.Scad.Utility
{
    public class ScadParser
    {
        public static List<int> GetModuleLineIndexes(string filePath)
        {
            var moduleIndexes = new List<int>();
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Trim().StartsWith("module "))
                    {
                        moduleIndexes.Add(i);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: File {filePath} not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
            return moduleIndexes;
        }

        public static GeneratedModule ParseModuleAndSettings(string filePath, int moduleLineIndex)
        {
            string[] lines;
            var decimalPlaces = 3; // for imperial conversions
            try
            {
                lines = File.ReadAllLines(filePath);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: File {filePath} not found.");
                return new GeneratedModule();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return new GeneratedModule();
            }

            if (moduleLineIndex < 0 || moduleLineIndex >= lines.Length)
            {
                Console.WriteLine($"Error: Invalid module line index {moduleLineIndex}.");
                return new GeneratedModule();
            }

            // Extract module name
            string moduleLine = lines[moduleLineIndex].Trim();
            string Module = "";
            if (moduleLine.StartsWith("module "))
            {
                int nameStart = "module ".Length;
                int nameEnd = moduleLine.IndexOf('(');
                if (nameEnd > nameStart)
                {
                    Module = $"{moduleLine.Substring(nameStart, nameEnd - nameStart).Trim()}();";
                }
            }

            // Get settings line (2 lines above)
            int settingsLineIndex = moduleLineIndex - 2;
            if (settingsLineIndex < 0)
            {
                Console.WriteLine("Error: No settings line found (index out of bounds).");
                return new GeneratedModule { CallingMethod = Module };
            }

            string settingsLine = lines[settingsLineIndex].Trim();
            if (!settingsLine.StartsWith("// Settings:"))
            {
                Console.WriteLine("Error: Settings line does not start with '// Settings:'.");
                return new GeneratedModule { CallingMethod = Module };
            }

            // Parse settings line
            var result = new GeneratedModule { CallingMethod = Module };
            string settingsContent = settingsLine.Substring("// Settings: ".Length);
            string[] pairs = settingsContent.Split([", "], StringSplitOptions.None);

            foreach (string pair in pairs)
            {
                if (pair.StartsWith("UnitSystem=")) // Get this value first to then check for conversions to imperial
                {
                    var _unit = pair.Substring("UnitSystem=".Length).Trim();
                    result.Unit = _unit == UnitSystem.Imperial.ToString() ? "in" : "mm";
                }
                else if (pair.StartsWith("BackgroundType="))
                {
                    result.Theme = pair.Substring("BackgroundType=".Length);
                }
                else if (pair.StartsWith("MinX="))
                {
                    if (double.TryParse(pair.Substring("MinX=".Length), out double minX))
                    {
                        // Imperial Conversion
                        if (result.Unit == "in")
                            minX = Math.Round(MillimeterToInches(minX), decimalPlaces);

                        result.MinX = minX;
                    }
                }
                else if (pair.StartsWith("MaxX="))
                {
                    if (double.TryParse(pair.Substring("MaxX=".Length), out double maxX))
                    {
                        // Imperial Conversion
                        if (result.Unit == "in")
                            maxX = Math.Round(MillimeterToInches(maxX), decimalPlaces);

                        result.MaxX = maxX;
                    }
                }
                else if (pair.StartsWith("MinY="))
                {
                    if (double.TryParse(pair.Substring("MinY=".Length), out double minY))
                    {
                        // Imperial Conversion
                        if (result.Unit == "in")
                            minY = Math.Round(MillimeterToInches(minY), decimalPlaces);

                        result.MinY = minY;
                    }
                }
                else if (pair.StartsWith("MaxY="))
                {
                    if (double.TryParse(pair.Substring("MaxY=".Length), out double maxY))
                    {
                        // Imperial Conversion
                        if (result.Unit == "in")
                            maxY = Math.Round(MillimeterToInches(maxY), decimalPlaces);

                        result.MaxY = maxY;
                    }
                }
                else if (pair.StartsWith("MinZ="))
                {
                    if (double.TryParse(pair.Substring("MinZ=".Length), out double minZ))
                    {
                        // Imperial Conversion
                        if (result.Unit == "in")
                            minZ = Math.Round(MillimeterToInches(minZ), decimalPlaces);

                        result.MinZ = minZ;
                    }
                }
                else if (pair.StartsWith("MaxZ="))
                {
                    if (double.TryParse(pair.Substring("MaxZ=".Length), out double maxZ))
                    {
                        // Imperial Conversion
                        if (result.Unit == "in")
                            maxZ = Math.Round(MillimeterToInches(maxZ), decimalPlaces);

                        result.MaxZ = maxZ;
                    }
                }
            }

            // Get volume if needed - cm or inches
            if (result.Unit == UnitSystem.Metric.ToString())
            {
                var (rangeX, rangeY, rangeZ, volume) = GetVolume(
                    minX: result.MinX,
                    maxX: result.MaxX,
                    minY: result.MinY,
                    maxY: result.MaxY,
                    minZ: result.MinZ,
                    maxZ: result.MaxZ,
                    unit: result.Unit);

                result.Volume = volume;
                result.RangeX = rangeX; 
                result.RangeY = rangeY; 
                result.RangeZ = rangeZ;
            }
            else
            {
                var (rangeX, rangeY, rangeZ, volume) = GetVolume(
                    minX: result.MinX,
                    maxX: result.MaxX,
                    minY: result.MinY,
                    maxY: result.MaxY,
                    minZ: result.MinZ,
                    maxZ: result.MaxZ,
                    unit: result.Unit);

                result.Volume = volume;
                result.RangeX = rangeX;
                result.RangeY = rangeY;
                result.RangeZ = rangeZ;
            }

            return result;
        }

        public ObservableCollection<GeneratedModule> AxesModulesList(string filePath)
        {
            var moduleIndexes = GetModuleLineIndexes(filePath);
            var ranges = new ObservableCollection<GeneratedModule>();

            foreach (int index in moduleIndexes)
            {
                var range = ParseModuleAndSettings(filePath, index);
                ranges.Add(range);
            }

            return ranges;
        }
    }
}
