using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class RoundedCube(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double SizeX => (double)_parameters["size_x"];
        public double SizeY => (double)_parameters["size_y"];
        public double SizeZ => (double)_parameters["size_z"];
        public double RoundRadius => _parameters.TryGetValue("round_r", out object? value) ? (double)value : 0;
        public double RoundHeight => _parameters.TryGetValue("round_h", out object? value) ? (double)value : 0;
        public int Resolution => _parameters.TryGetValue("resolution", out object? value) ? (int)value : 360;

        private Cube AdjustedCube => new(new Dictionary<string, object>
        {
            { "size_x", Math.Max(0, SizeX - 2 * RoundRadius) },
            { "size_y", Math.Max(0, SizeY - 2 * RoundRadius) },
            { "size_z", SizeZ }
        });

        private Cylinder RoundingCylinder => new(new Dictionary<string, object>
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
    }
}
