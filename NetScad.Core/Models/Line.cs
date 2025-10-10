using NetScad.Core.Interfaces;
using System.Collections.Generic;

namespace NetScad.Core.Models
{
    public partial class Line : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters;

        public Line(Dictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

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

        // Client-side example:
        /*
        var lineParams = new Dictionary<string, object> { { "x1", 0.0 }, { "y1", 0.0 }, { "x2", 10.0 }, { "y2", 10.0 } };
        var line = OScad1D.Line.ToScadObject(lineParams);
        Console.WriteLine(line.OSCADMethod); // line([0,0], [10,10]);
        var dbData = line.ToDbDictionary(); // { "type": "Line", "x1": 0, "y1": 0, "x2": 10, "y2": 10 }
        // SQLite: INSERT INTO Models (Type, X1, Y1, X2, Y2) VALUES ('Line', 0, 0, 10, 10);
        */
    }
}
