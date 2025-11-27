using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
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
    }
}
