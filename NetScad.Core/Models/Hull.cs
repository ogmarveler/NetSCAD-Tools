using NetScad.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace NetScad.Core.Models
{
    public partial class Hull : IScadObject, IDbSerializable
    {
        private readonly IScadObject[] _children;

        public Hull(params IScadObject[] children)
        {
            _children = children;
        }

        public IScadObject[] Children => _children;

        public string OSCADMethod => $"hull() {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Hull" }
        };

        // Client-side example:
        /*
        var cube = OScad3D.Cube.ToScadObject(new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } });
        var hullParams = new Dictionary<string, object> { { "children", new IScadObject[] { cube } } };
        var hull = OScadBooleanOperation.Hull.ToScadObject(hullParams);
        Console.WriteLine(hull.OSCADMethod); // hull() { cube([10, 20, 30]); };
        var dbData = hull.ToDbDictionary(); // { "type": "Hull" }
        // SQLite: INSERT INTO Models (Type) VALUES ('Hull');
        // Store children in a separate Children table
        */
    }
}
