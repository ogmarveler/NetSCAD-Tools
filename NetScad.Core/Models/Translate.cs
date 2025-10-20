using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public partial class Translate(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double X => (double)_parameters["x"];
        public double Y => (double)_parameters["y"];
        public double Z => (double)_parameters["z"];
        public IScadObject[] Children => _parameters.TryGetValue("children", out object? value) ? (IScadObject[])value : [];

        public string OSCADMethod => $"translate([{X}, {Y}, {Z}]) {{ {string.Join(" ", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Translate" },
            { "x", X },
            { "y", Y },
            { "z", Z }
        };

        // Client-side example:
        /*
        var cube = OScad3D.Cube.ToScadObject(new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } });
        var transParams = new Dictionary<string, object> { { "x", 10.0 }, { "y", 20.0 }, { "z", 30.0 }, { "children", new IScadObject[] { cube } } };
        var translate = OScadTransform.Translate.ToScadObject(transParams);
        Console.WriteLine(translate.OSCADMethod); // translate([10, 20, 30]) { cube([10, 20, 30]); };
        var dbData = translate.ToDbDictionary(); // { "type": "Translate", "x": 10, "y": 20, "z": 30 }
        // SQLite: INSERT INTO Models (Type, X, Y, Z) VALUES ('Translate', 10, 20, 30);
        // Store children in a separate Children table with foreign keys
        */
    }
}
