namespace NetScad.Axis.Scad.Utility
{
    public static class StringFunctions
    {
        public static string FormatAxisDisplay(string? axisUsed)
        {
            if (axisUsed == "Select Axis") return "Select Axis";

            string formatted = $"{axisUsed?
                        .Replace("use <../Axes/axes.scad>; ", "")
                        .Replace("Get_", "")
                        .Replace("_", " ")
                        .Replace("();", "")
                        .Replace(" Orig ", ", Origin: ")
                        .Replace("N", "-", comparisonType: StringComparison.InvariantCulture)
                        .Replace("MM", "mm")
                        .Replace("Inch", "in")
                        .Replace("x", " x ")}"; // Format for display
            
            // Trim from ", Origin: " to the end of the string
            int originIndex = formatted.IndexOf(", Origin: ");
            if (originIndex >= 0)
            {
                return formatted.Substring(0, originIndex);
            }
            return formatted;
        }
    }
}
