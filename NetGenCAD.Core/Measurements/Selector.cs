using System.ComponentModel;

namespace NetGenCAD.Core.Measurements
{
    public partial class Selector
    {
        public enum UnitSystem
        {
            [Description("Metric (mm)")]
            Metric,
            [Description("Imperial (in)")]
            Imperial
        }

        public enum BackgroundType
        {
            [Description("Light Theme")]
            Light,
            [Description("Dark Theme")]
            Dark
        }

        public static List<string> ScrewProperties { get; } = new List<string> { "Screw Thread", "Screw Head", "Threaded Insert", "Clearance Hole" };
        public static List<string> ServerRackWidthTypes { get; } = new List<string> { "Inner Mount", "Outer Mount" };
        public static List<string> SolidTypes { get; } = new List<string> { "Cube", "Round Cube", "Cylinder", "Round Cylinder", "Polyhedron", "Sphere", "Surface", "Text" };
        public static List<string> TextAlignOptions { get; } = new List<string> { "Left", "Center", "Right" };
        public static List<string> TextDirectionOptions { get; } = new List<string> { "L-to-R", "R-to-L" };
        public static List<string> VerticalAlignOptions { get; } = new List<string> { "Top", "Center", "Baseline", "Bottom" };

        public static readonly string[] PortableFonts =
        {
                "Liberation Sans",
                "Liberation Serif",
                "Liberation Mono"
        };

        public static readonly string[] PortableFontsWithStyles =
        {
                // Sans
                "Liberation Sans",
                "Liberation Sans:style=Bold",
                "Liberation Sans:style=Italic",
                "Liberation Sans:style=Bold Italic",
                // Serif
                "Liberation Serif",
                "Liberation Serif:style=Bold",
                "Liberation Serif:style=Italic",
                "Liberation Serif:style=Bold Italic",
                // Mono
                "Liberation Mono",
                "Liberation Mono:style=Bold",
                "Liberation Mono:style=Oblique",
                "Liberation Mono:style=Bold Oblique"
        };
    }
}
