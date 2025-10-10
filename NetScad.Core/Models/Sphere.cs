using NetScad.Core.Interfaces;
using System.Collections.Generic;

namespace NetScad.Core.Models
{
    public partial class Sphere : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters;

        public Sphere(Dictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public double Radius => (double)_parameters["r"];
        public double Resolution => _parameters.ContainsKey("resolution") ? (double)_parameters["resolution"] : 100;

        public string OSCADMethod => $"sphere(r = {Radius}, $fn = {Resolution});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Sphere" },
            { "r", Radius },
            { "resolution", Resolution }
        };

        // Client-side example:
        /*
        var sphereParams = new Dictionary<string, object> { { "r", 5.0 }, { "resolution", 100.0 } };
        var sphere = OScad3D.Sphere.ToScadObject(sphereParams);
        Console.WriteLine(sphere.OSCADMethod); // sphere(r=5, $fn=100);
        var dbData = sphere.ToDbDictionary(); // { "type": "Sphere", "r": 5, "resolution": 100 }
        // SQLite: INSERT INTO Models (Type, Radius, Resolution) VALUES ('Sphere', 5, 100);
        */
    }
}
