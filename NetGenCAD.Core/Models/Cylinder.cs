using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Cylinder(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double Radius => (double)_parameters["r"];
        public double Height => (double)_parameters["h"];
        public double Radius1 => _parameters.ContainsKey("r1") ? Convert.ToDouble(_parameters["r1"]) : 0;
        public double Radius2 => _parameters.ContainsKey("r2") ? Convert.ToDouble(_parameters["r2"]) : 0;
        public int Resolution => 360;

        public string OSCADMethod => $"cylinder(h = {Height}, {(Radius1 == 0.0 && Radius2 == 0.0 ? $"r = {Radius}" : $"r1 = {(Radius1 == 0 ? Radius : Radius1)}, r2 = {(Radius2 == 0 ? Radius : Radius2)}")}, $fn = {Resolution});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Cylinder" },
            { "r", Radius },
            { "h", Height },
            { "r1", Radius1 },
            { "r2", Radius2 },
            { "resolution", Resolution }
        };
    }
}
