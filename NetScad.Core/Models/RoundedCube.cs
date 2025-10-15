using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public partial class RoundedCube : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters;

        public RoundedCube(Dictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public double SizeX => (double)_parameters["size_x"];
        public double SizeY => (double)_parameters["size_y"];
        public double SizeZ => (double)_parameters["size_z"];
        public double RoundRadius => (double)_parameters["round_r"];
        public double RoundHeight => _parameters.ContainsKey("round_h") ? (double)_parameters["round_h"] : 0.001;
        public double Resolution => _parameters.ContainsKey("resolution") ? (double)_parameters["resolution"] : 200;

        private Cube AdjustedCube => new Cube(new Dictionary<string, object>
        {
            { "size_x", Math.Max(0, SizeX - 2 * RoundRadius) },
            { "size_y", Math.Max(0, SizeY - 2 * RoundRadius) },
            { "size_z", SizeZ }
        });

        private Cylinder RoundingCylinder => new Cylinder(new Dictionary<string, object>
        {
            { "r", RoundRadius },
            { "h", RoundHeight },
            { "resolution", Resolution }
        });

        public string OSCADMethod => new Minkowski(AdjustedCube, RoundingCylinder).OSCADMethod;

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "RoundedCube" },
            { "size_x", SizeX },
            { "size_y", SizeY },
            { "size_z", SizeZ },
            { "round_r", RoundRadius },
            { "round_h", RoundHeight },
            { "resolution", Resolution }
        };

        // Client-side example:
        /*
        var roundedCubeParams = new Dictionary<string, object>
        {
            { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 }, { "round_r", 2.0 }, { "round_h", 0.002 }, { "resolution", 150.0 }
        };
        var roundedCube = OScad3D.RoundedCube.ToScadObject(roundedCubeParams);
        Console.WriteLine(roundedCube.OSCADMethod); // minkowski() { cube([6, 16, 30]); cylinder(r=2, h=0.002, $fn=150); };
        var dbData = roundedCube.ToDbDictionary(); // { "type": "RoundedCube", "size_x": 10, "size_y": 20, "size_z": 30, "round_r": 2, "round_h": 0.002, "resolution": 150 }
        // SQLite: INSERT INTO Models (Type, SizeX, SizeY, SizeZ, RoundRadius, RoundHeight, Resolution) VALUES ('RoundedCube', 10, 20, 30, 2, 0.002, 150);
        */
    }
}
