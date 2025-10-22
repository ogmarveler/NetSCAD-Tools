namespace NetScad.Axis.Scad.Utility
{
    public static class StringFunctions
    {
        public static string FormatAxisDisplay(string? axisUsed)
        {
            return $"{axisUsed?
                        .Replace("use <../Axes/axes.scad>; ", "")
                        .Replace("Get_", "")
                        .Replace("_", " ")
                        .Replace("();", "")
                        .Replace(" Orig ", ", Origin: ")
                        .Replace("N", "-", comparisonType: StringComparison.InvariantCulture)
                        .Replace("Light ", "")
                        .Replace("Dark ", "")
                        .Replace("MM", "mm")
                        .Replace("Inch", "in")
                        .Replace("x", " x ")}"; // Format for display
        }
    }
}
