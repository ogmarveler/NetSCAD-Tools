using NetScad.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace NetScad.Core.Models
{
    public partial class Minkowski : IScadObject, IDbSerializable
    {
        private readonly IScadObject[] _children;

        public Minkowski(params IScadObject[] children)
        {
            _children = children;
        }

        public IScadObject[] Children => _children;

        public string OSCADMethod => $"minkowski() {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Minkowski" }
        };

        // Client-side example:
        /*
        var cube = OScad3D.Cube.ToScadObject(new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } });
        var minkowskiParams = new Dictionary<string, object> { { "children", new IScadObject[] { cube } } };
        var minkowski = OScadBooleanOperation.Minkowski.ToScadObject(minkowskiParams);
        Console.WriteLine(minkowski.OSCADMethod); // minkowski() { cube([10, 20, 30]); };
        var dbData = minkowski.ToDbDictionary(); // { "type": "Minkowski" }
        // SQLite: INSERT INTO Models (Type) VALUES ('Minkowski');
        // Store children in a separate Children table
        */
    }
}
