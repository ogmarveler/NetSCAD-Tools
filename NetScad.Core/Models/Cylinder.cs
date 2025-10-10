using NetScad.Core.Interfaces;
using System.Collections.Generic;

namespace NetScad.Core.Models
{
    public partial class Cylinder : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters;

        public Cylinder(Dictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public double Radius => (double)_parameters["r"];
        public double Height => (double)_parameters["h"];
        public double? Radius1 => _parameters.ContainsKey("r1") ? (double)_parameters["r1"] : null;
        public double? Radius2 => _parameters.ContainsKey("r2") ? (double)_parameters["r2"] : null;
        public double Resolution => _parameters.ContainsKey("resolution") ? (double)_parameters["resolution"] : 100;

        public string OSCADMethod => $"cylinder(h = {Height}, {(Radius1 == null && Radius2 == null ? $"r = {Radius}" : $"r1 = {Radius1 ?? Radius}, r2 = {Radius2 ?? Radius}")}, $fn = {Resolution});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Cylinder" },
            { "r", Radius },
            { "h", Height },
            { "r1", Radius1 },
            { "r2", Radius2 },
            { "resolution", Resolution }
        };

        // Client-side example:
        /*
        var cylParams = new Dictionary<string, object> { { "r", 5.0 }, { "h", 10.0 }, { "resolution", 50.0 } };
        var cylinder = OScad3D.Cylinder.ToScadObject(cylParams);
        Console.WriteLine(cylinder.OSCADMethod); // cylinder(h=10, r=5, $fn=50);
        var dbData = cylinder.ToDbDictionary(); // { "type": "Cylinder", "r": 5, "h": 10, "r1": null, "r2": null, "resolution": 50 }
        // SQLite: INSERT INTO Models (Type, Radius, Height, Radius1, Radius2, Resolution) VALUES ('Cylinder', 5, 10, NULL, NULL, 50);
        */
    }
}
