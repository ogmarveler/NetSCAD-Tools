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
        public static List<string> SolidTypes { get; } = new List<string> { "Cube", "Round Cube", "Cylinder", "Round Cylinder", "Sphere", "Surface" };
    }
}
