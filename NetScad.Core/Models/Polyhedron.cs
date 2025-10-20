using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
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

        // Client-side example:
        /*
        var polyParams = new Dictionary<string, object>
        {
            { "points", new List<List<double>> { new() { 0, 0, 0 }, new() { 1, 0, 0 }, new() { 0, 1, 0 }, new() { 0, 0, 1 } } },
            { "faces", new List<List<int>> { new() { 0, 1, 2 }, new() { 0, 2, 3 }, new() { 0, 3, 1 }, new() { 1, 3, 2 } } },
            { "convexity", 1 }
        };
        var polyhedron = OScad3D.Polyhedron.ToScadObject(polyParams);
        Console.WriteLine(polyhedron.OSCADMethod); // polyhedron(points=[[0,0,0],[1,0,0],[0,1,0],[0,0,1]], faces=[[0,1,2],[0,2,3],[0,3,1],[1,3,2]], convexity=1);
        var dbData = polyhedron.ToDbDictionary(); // { "type": "Polyhedron", "convexity": 1 }
        // SQLite: INSERT INTO Models (Type, Convexity) VALUES ('Polyhedron', 1);
        // Store points/faces in separate Points and Faces tables with foreign keys
        */
    }
}
