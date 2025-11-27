using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Circle(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double Radius => (double)_parameters["r"];
        public double Resolution => _parameters.TryGetValue("resolution", out object? value) ? (double)value : 100;

        public string OSCADMethod => $"circle(r = {Radius}, $fn = {Resolution});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Circle" },
            { "r", Radius },
            { "resolution", Resolution }
        };
    }
}
