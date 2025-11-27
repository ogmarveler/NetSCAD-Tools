using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
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
    }
}
