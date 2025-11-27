using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Arc(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double Radius => (double)_parameters["r"];
        public double Start => (double)_parameters["start"];
        public double End => (double)_parameters["end"];
        public double Resolution => _parameters.TryGetValue("resolution", out object? value) ? (double)value : 100;

        public string OSCADMethod => $"arc(r = {Radius}, start = {Start}, end = {End}, $fn = {Resolution});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Arc" },
            { "r", Radius },
            { "start", Start },
            { "end", End },
            { "resolution", Resolution }
        };
    }
}
