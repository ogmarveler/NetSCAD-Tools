using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public partial class Resize(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double RX => (double)_parameters["rx"];
        public double RY => (double)_parameters["ry"];
        public double RZ => (double)_parameters["rz"];
        public bool Auto => _parameters.TryGetValue("auto", out object? value) && (bool)value;
        public IScadObject[] Children => _parameters.TryGetValue("children", out object? value) ? (IScadObject[])value : [];

        public string OSCADMethod => $"resize([{RX}, {RY}, {RZ}], auto = {Auto.ToString().ToLower()}) {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Resize" },
            { "rx", RX },
            { "ry", RY },
            { "rz", RZ },
            { "auto", Auto }
        };

        // Client-side example:
        /*
        var cube = OScad3D.Cube.ToScadObject(new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } });
        var resizeParams = new Dictionary<string, object> { { "rx", 20.0 }, { "ry", 40.0 }, { "rz", 60.0 }, { "auto", false }, { "children", new IScadObject[] { cube } } };
        var resize = OScadTransform.Resize.ToScadObject(resizeParams);
        Console.WriteLine(resize.OSCADMethod); // resize([20, 40, 60], auto=false) { cube([10, 20, 30]); };
        var dbData = resize.ToDbDictionary(); // { "type": "Resize", "rx": 20, "ry": 40, "rz": 60, "auto": false }
        // SQLite: INSERT INTO Models (Type, RX, RY, RZ, Auto) VALUES ('Resize', 20, 40, 60, 0);
        // Store children in a separate Children table
        */
    }
}
