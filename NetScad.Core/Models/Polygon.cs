using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
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

        // Client-side example:
        /*
        var polyParams = new Dictionary<string, object>
        {
            { "points", new List<List<double>> { new() { 0, 0 }, new() { 10, 0 }, new() { 0, 10 } } },
            { "paths", new List<List<int>> { new() { 0, 1, 2 } } },
            { "convexity", 1 }
        };
        var polygon = OScad2D.Polygon.ToScadObject(polyParams);
        Console.WriteLine(polygon.OSCADMethod); // polygon(points=[[0,0],[10,0],[0,10]], paths=[[0,1,2]], convexity=1);
        var dbData = polygon.ToDbDictionary(); // { "type": "Polygon", "convexity": 1 }
        // SQLite: INSERT INTO Models (Type, Convexity) VALUES ('Polygon', 1);
        // Store points/paths in separate Points and Paths tables
        */
    }
}
