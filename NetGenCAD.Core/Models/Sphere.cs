using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Sphere(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double Radius => (double)_parameters["r"];
        public int Resolution => _parameters.TryGetValue("resolution", out object? value) ? (int)value : 360;

        public string OSCADMethod => $"sphere(r = {Radius}, $fn = {Resolution});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Sphere" },
            { "r", Radius },
            { "resolution", Resolution }
        };
    }
}
