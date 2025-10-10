using NetScad.Core.Interfaces;
using System.Collections.Generic;

namespace NetScad.Core.Models
{
    public partial class RoundedPolyhedron : IScadObject, IDbSerializable
    {
        private readonly List<List<double>> _points;
        private readonly List<List<int>> _faces;
        private readonly double _roundRadius;
        private readonly double _roundHeight;
        private readonly double _resolution;

        public RoundedPolyhedron(List<List<double>> points, List<List<int>> faces, double round_r, double round_h = 0.001, double resolution = 200)
        {
            _points = points;
            _faces = faces;
            _roundRadius = round_r;
            _roundHeight = round_h;
            _resolution = resolution;
        }

        public List<List<double>> Points => _points;
        public List<List<int>> Faces => _faces;
        public double RoundRadius => _roundRadius;
        public double RoundHeight => _roundHeight;
        public double Resolution => _resolution;

        private Polyhedron AdjustedPolyhedron => new Polyhedron(Points, Faces, 1);

        private Cylinder RoundingCylinder => new Cylinder(new Dictionary<string, object>
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

        // Client-side example:
        /*
        var roundedPolyParams = new Dictionary<string, object>
        {
            { "points", new List<List<double>> { new() { 0, 0, 0 }, new() { 1, 0, 0 }, new() { 0, 1, 0 }, new() { 0, 0, 1 } } },
            { "faces", new List<List<int>> { new() { 0, 1, 2 }, new() { 0, 2, 3 }, new() { 0, 3, 1 }, new() { 1, 3, 2 } } },
            { "round_r", 1.0 }, { "round_h", 0.002 }, { "resolution", 200.0 }
        };
        var roundedPoly = OScad3D.RoundedPolyhedron.ToScadObject(roundedPolyParams);
        Console.WriteLine(roundedPoly.OSCADMethod); // minkowski() { polyhedron(...); cylinder(r=1, h=0.002, $fn=200); };
        var dbData = roundedPoly.ToDbDictionary(); // { "type": "RoundedPolyhedron", "round_r": 1, "round_h": 0.002, "resolution": 200 }
        // SQLite: INSERT INTO Models (Type, RoundRadius, RoundHeight, Resolution) VALUES ('RoundedPolyhedron', 1, 0.002, 200);
        // Store points/faces in separate Points and Faces tables
        */
    }
}
