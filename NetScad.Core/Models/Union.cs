using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public partial class Union(params IScadObject[] children) : IScadObject, IDbSerializable
    {
        private readonly IScadObject[] _children = children;

        public IScadObject[] Children => _children;

        public string OSCADMethod => $"union() {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Union" }
        };

        // Client-side example:
        /*
        var cube = OScad3D.Cube.ToScadObject(new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } });
        var unionParams = new Dictionary<string, object> { { "children", new IScadObject[] { cube } } };
        var union = OScadBooleanOperation.Union.ToScadObject(unionParams);
        Console.WriteLine(union.OSCADMethod); // union() { cube([10, 20, 30]); };
        var dbData = union.ToDbDictionary(); // { "type": "Union" }
        // SQLite: INSERT INTO Models (Type) VALUES ('Union');
        // Store children in a separate Children table
        */
    }
}
