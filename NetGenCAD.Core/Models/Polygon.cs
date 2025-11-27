using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Polygon(List<List<double>> points, List<List<int>>? paths = null, int convexity = 1) : IScadObject, IDbSerializable
    {
        private readonly List<List<double>> _points = points;
        private readonly List<List<int>>? _paths = paths;
        private readonly int _convexity = convexity;

        public List<List<double>> Points => _points;
        public List<List<int>>? Paths => _paths;
        public int Convexity => _convexity;

        public string OSCADMethod => $"polygon(points = [{string.Join(", ", Points.Select(inner => $"[{string.Join(", ", inner)}]"))}]{(Paths != null ? $", paths = [{string.Join(", ", Paths.Select(inner => $"[{string.Join(", ", inner)}]"))}]" : "")}, convexity = {Convexity});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Polygon" },
            { "convexity", Convexity }
        };
    }
}
