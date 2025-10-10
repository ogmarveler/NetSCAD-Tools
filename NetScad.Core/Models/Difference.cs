using NetScad.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace NetScad.Core.Models
{
    public partial class Difference : IScadObject, IDbSerializable
    {
        private readonly IScadObject[] _children;

        public Difference(params IScadObject[] children)
        {
            _children = children;
        }

        public IScadObject[] Children => _children;

        public string OSCADMethod => $"difference() {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Difference" }
        };

        // Client-side example:
        /*
        var cube = OScad3D.Cube.ToScadObject(new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } });
        var diffParams = new Dictionary<string, object> { { "children", new IScadObject[] { cube } } };
        var difference = OScadBooleanOperation.Difference.ToScadObject(diffParams);
        Console.WriteLine(difference.OSCADMethod); // difference() { cube([10, 20, 30]); };
        var dbData = difference.ToDbDictionary(); // { "type": "Difference" }
        // SQLite: INSERT INTO Models (Type) VALUES ('Difference');
        // Store children in a separate Children table
        */
    }
}
