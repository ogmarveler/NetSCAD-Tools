using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public partial class Cube(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

        public double SizeX => (double)_parameters["size_x"];
        public double SizeY => (double)_parameters["size_y"];
        public double SizeZ => (double)_parameters["size_z"];

        public string OSCADMethod => $"cube([{SizeX}, {SizeY}, {SizeZ}]);";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Cube" },
            { "size_x", SizeX },
            { "size_y", SizeY },
            { "size_z", SizeZ }
        };

        // Client-side example:
        /*
        var cubeParams = new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } };
        var cube = OScad3D.Cube.ToScadObject(cubeParams);
        Console.WriteLine(cube.OSCADMethod); // cube([10, 20, 30]);
        var dbData = cube.ToDbDictionary(); // { "type": "Cube", "size_x": 10, "size_y": 20, "size_z": 30 }
        // SQLite: INSERT INTO Models (Type, SizeX, SizeY, SizeZ) VALUES ('Cube', 10, 20, 30);
        */
    }
}
