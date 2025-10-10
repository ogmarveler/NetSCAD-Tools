using NetScad.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetScad.Core.Models
{
    public partial class Rotate : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters;

        public Rotate(Dictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public double AX => (double)_parameters["ax"];
        public double AY => (double)_parameters["ay"];
        public double AZ => (double)_parameters["az"];
        public IScadObject[] Children => _parameters.ContainsKey("children") ? (IScadObject[])_parameters["children"] : Array.Empty<IScadObject>();

        public string OSCADMethod => $"rotate([{AX}, {AY}, {AZ}]) {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Rotate" },
            { "ax", AX },
            { "ay", AY },
            { "az", AZ }
        };

        // Client-side example:
        /*
        var cube = OScad3D.Cube.ToScadObject(new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } });
        var rotateParams = new Dictionary<string, object> { { "ax", 45.0 }, { "ay", 0.0 }, { "az", 0.0 }, { "children", new IScadObject[] { cube } } };
        var rotate = OScadTransform.Rotate.ToScadObject(rotateParams);
        Console.WriteLine(rotate.OSCADMethod); // rotate([45, 0, 0]) { cube([10, 20, 30]); };
        var dbData = rotate.ToDbDictionary(); // { "type": "Rotate", "ax": 45, "ay": 0, "az": 0 }
        // SQLite: INSERT INTO Models (Type, AX, AY, AZ) VALUES ('Rotate', 45, 0, 0);
        // Store children in a separate Children table
        */
    }
}