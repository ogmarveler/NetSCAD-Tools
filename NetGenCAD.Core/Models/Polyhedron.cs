using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Polyhedron(List<List<double>> points, List<List<int>> faces, int convexity = 1) : IScadObject, IDbSerializable
    {
        private readonly List<List<double>> _points = points;
        private readonly List<List<int>> _faces = faces;
        private readonly int _convexity = convexity;

        public List<List<double>> Points => _points;
        public List<List<int>> Faces => _faces;
        public int Convexity => _convexity;

        public string OSCADMethod => $"polyhedron(points = [{string.Join(", ", Points.Select(inner => $"[{string.Join(", ", inner)}]"))}], faces = [{string.Join(", ", Faces.Select(inner => $"[{string.Join(", ", inner)}]"))}], convexity = {Convexity});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Polyhedron" },
            { "convexity", Convexity }
        };
    }
}
