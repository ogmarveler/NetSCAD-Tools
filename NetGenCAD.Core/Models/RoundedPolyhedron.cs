using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class RoundedPolyhedron(List<List<double>> points, List<List<int>> faces, double round_r, double round_h = 0.001, double resolution = 200) : IScadObject, IDbSerializable
    {
        private readonly List<List<double>> _points = points;
        private readonly List<List<int>> _faces = faces;
        private readonly double _roundRadius = round_r;
        private readonly double _roundHeight = round_h;
        private readonly double _resolution = resolution;

        public List<List<double>> Points => _points;
        public List<List<int>> Faces => _faces;
        public double RoundRadius => _roundRadius;
        public double RoundHeight => _roundHeight;
        public double Resolution => _resolution;

        private Polyhedron AdjustedPolyhedron => new(Points, Faces, 1);

        private Cylinder RoundingCylinder => new(new Dictionary<string, object>
        {
            { "r", RoundRadius },
            { "h", RoundHeight },
            { "resolution", Resolution }
        });

        public string OSCADMethod => new Minkowski(AdjustedPolyhedron, RoundingCylinder).OSCADMethod;

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "RoundedPolyhedron" },
            { "round_r", RoundRadius },
            { "round_h", RoundHeight },
            { "resolution", Resolution }
        };
    }
}
