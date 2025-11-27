using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Scale(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double SX => (double)_parameters["sx"];
        public double SY => (double)_parameters["sy"];
        public double SZ => (double)_parameters["sz"];
        public IScadObject[] Children => _parameters.TryGetValue("children", out object? value) ? (IScadObject[])value : [];

        public string OSCADMethod => $"scale([{SX}, {SY}, {SZ}]) {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Scale" },
            { "sx", SX },
            { "sy", SY },
            { "sz", SZ }
        };
    }
}
