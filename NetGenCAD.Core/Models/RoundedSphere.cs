using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class RoundedSphere(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double Radius => (double)_parameters["r"];
        public double RoundRadius => (double)_parameters["round_r"];
        public double RoundHeight => _parameters.TryGetValue("round_h", out object? value) ? (double)value : 0.001;
        public double Resolution => _parameters.TryGetValue("resolution", out object? value) ? (double)value : 200;

        private Sphere AdjustedSphere => new(new Dictionary<string, object>
        {
            { "r", Math.Max(0, Radius - RoundRadius) },
            { "resolution", Resolution }
        });

        private Cylinder RoundingCylinder => new(new Dictionary<string, object>
        {
            { "r", RoundRadius },
            { "h", RoundHeight },
            { "resolution", Resolution }
        });

        public string OSCADMethod => new Minkowski(AdjustedSphere, RoundingCylinder).OSCADMethod;

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "RoundedSphere" },
            { "r", Radius },
            { "round_r", RoundRadius },
            { "round_h", RoundHeight },
            { "resolution", Resolution }
        };
    }
}
