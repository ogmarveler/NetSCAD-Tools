using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
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
    }
}
