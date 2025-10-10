using NetScad.Core.Interfaces;
using System.Collections.Generic;

namespace NetScad.Core.Models
{
    public partial class Arc : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters;

        public Arc(Dictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public double Radius => (double)_parameters["r"];
        public double Start => (double)_parameters["start"];
        public double End => (double)_parameters["end"];
        public double Resolution => _parameters.ContainsKey("resolution") ? (double)_parameters["resolution"] : 100;

        public string OSCADMethod => $"arc(r = {Radius}, start = {Start}, end = {End}, $fn = {Resolution});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Arc" },
            { "r", Radius },
            { "start", Start },
            { "end", End },
            { "resolution", Resolution }
        };

        // Client-side example:
        /*
        var arcParams = new Dictionary<string, object> { { "r", 5.0 }, { "start", 0.0 }, { "end", 90.0 }, { "resolution", 100.0 } };
        var arc = OScad1D.Arc.ToScadObject(arcParams);
        Console.WriteLine(arc.OSCADMethod); // arc(r=5, start=0, end=90, $fn=100);
        var dbData = arc.ToDbDictionary(); // { "type": "Arc", "r": 5, "start": 0, "end": 90, "resolution": 100 }
        // SQLite: INSERT INTO Models (Type, Radius, Start, End, Resolution) VALUES ('Arc', 5, 0, 90, 100);
        */
    }
}
