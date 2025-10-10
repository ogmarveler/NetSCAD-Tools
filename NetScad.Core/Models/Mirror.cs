using NetScad.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetScad.Core.Models
{
    public partial class Mirror : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters;

        public Mirror(Dictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public double MX => (double)_parameters["mx"];
        public double MY => (double)_parameters["my"];
        public double MZ => (double)_parameters["mz"];
        public IScadObject[] Children => _parameters.ContainsKey("children") ? (IScadObject[])_parameters["children"] : Array.Empty<IScadObject>();

        public string OSCADMethod => $"mirror([{MX}, {MY}, {MZ}]) {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Mirror" },
            { "mx", MX },
            { "my", MY },
            { "mz", MZ }
        };

        // Client-side example:
        /*
        var cube = OScad3D.Cube.ToScadObject(new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } });
        var mirrorParams = new Dictionary<string, object> { { "mx", 1.0 }, { "my", 0.0 }, { "mz", 0.0 }, { "children", new IScadObject[] { cube } } };
        var mirror = OScadTransform.Mirror.ToScadObject(mirrorParams);
        Console.WriteLine(mirror.OSCADMethod); // mirror([1, 0, 0]) { cube([10, 20, 30]); };
        var dbData = mirror.ToDbDictionary(); // { "type": "Mirror", "mx": 1, "my": 0, "mz": 0 }
        // SQLite: INSERT INTO Models (Type, MX, MY, MZ) VALUES ('Mirror', 1, 0, 0);
        // Store children in a separate Children table
        */
    }
}
