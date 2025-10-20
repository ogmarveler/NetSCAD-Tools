using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public partial class Intersection(params IScadObject[] children) : IScadObject, IDbSerializable
    {
        private readonly IScadObject[] _children = children;

        public IScadObject[] Children => _children;

        public string OSCADMethod => $"intersection() {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Intersection" }
        };

        // Client-side example:
        /*
        var cube = OScad3D.Cube.ToScadObject(new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } });
        var interParams = new Dictionary<string, object> { { "children", new IScadObject[] { cube } } };
        var intersection = OScadBooleanOperation.Intersection.ToScadObject(interParams);
        Console.WriteLine(intersection.OSCADMethod); // intersection() { cube([10, 20, 30]); };
        var dbData = intersection.ToDbDictionary(); // { "type": "Intersection" }
        // SQLite: INSERT INTO Models (Type) VALUES ('Intersection');
        // Store children in a separate Children table
        */
    }
}
