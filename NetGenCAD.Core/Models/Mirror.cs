using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Mirror(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double MX => (double)_parameters["mx"];
        public double MY => (double)_parameters["my"];
        public double MZ => (double)_parameters["mz"];
        public IScadObject[] Children => _parameters.TryGetValue("children", out object? value) ? (IScadObject[])value : [];

        public string OSCADMethod => $"mirror([{MX}, {MY}, {MZ}]) {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Mirror" },
            { "mx", MX },
            { "my", MY },
            { "mz", MZ }
        };
    }
}
