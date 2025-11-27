using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Square(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double SizeX => (double)_parameters["size_x"];
        public double SizeY => (double)_parameters["size_y"];
        public bool Center => _parameters.ContainsKey("center") && (bool)_parameters["center"];

        public string OSCADMethod => $"square([{SizeX}, {SizeY}]{(Center ? $", center = {Center.ToString().ToLower()}" : "")});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Square" },
            { "size_x", SizeX },
            { "size_y", SizeY },
            { "center", Center }
        };
    }
}
