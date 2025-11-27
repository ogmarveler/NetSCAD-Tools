using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Rotate(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double AX => (double)_parameters["ax"];
        public double AY => (double)_parameters["ay"];
        public double AZ => (double)_parameters["az"];
        public IScadObject[] Children => _parameters.TryGetValue("children", out object? value) ? (IScadObject[])value : [];

        public string OSCADMethod => $"rotate([{AX}, {AY}, {AZ}]) {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Rotate" },
            { "ax", AX },
            { "ay", AY },
            { "az", AZ }
        };
    }
}