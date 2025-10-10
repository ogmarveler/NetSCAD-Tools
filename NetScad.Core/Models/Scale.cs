using NetScad.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetScad.Core.Models
{
    public partial class Scale : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters;

        public Scale(Dictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public double SX => (double)_parameters["sx"];
        public double SY => (double)_parameters["sy"];
        public double SZ => (double)_parameters["sz"];
        public IScadObject[] Children => _parameters.ContainsKey("children") ? (IScadObject[])_parameters["children"] : Array.Empty<IScadObject>();

        public string OSCADMethod => $"scale([{SX}, {SY}, {SZ}]) {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Scale" },
            { "sx", SX },
            { "sy", SY },
            { "sz", SZ }
        };

        // Client-side example:
        /*
        var cube = OScad3D.Cube.ToScadObject(new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } });
        var scaleParams = new Dictionary<string, object> { { "sx", 2.0 }, { "sy", 2.0 }, { "sz", 2.0 }, { "children", new IScadObject[] { cube } } };
        var scale = OScadTransform.Scale.ToScadObject(scaleParams);
        Console.WriteLine(scale.OSCADMethod); // scale([2, 2, 2]) { cube([10, 20, 30]); };
        var dbData = scale.ToDbDictionary(); // { "type": "Scale", "sx": 2, "sy": 2, "sz": 2 }
        // SQLite: INSERT INTO Models (Type, SX, SY, SZ) VALUES ('Scale', 2, 2, 2);
        // Store children in a separate Children table
        */
    }
}
