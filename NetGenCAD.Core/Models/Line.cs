using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Line(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double X1 => (double)_parameters["x1"];
        public double Y1 => (double)_parameters["y1"];
        public double X2 => (double)_parameters["x2"];
        public double Y2 => (double)_parameters["y2"];

        public string OSCADMethod => $"line([{X1}, {Y1}], [{X2}, {Y2}]);";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Line" },
            { "x1", X1 },
            { "y1", Y1 },
            { "x2", X2 },
            { "y2", Y2 }
        };
    }
}
