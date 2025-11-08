using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public partial class RoundedCylinder(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double Radius => (double)_parameters["r"];
        public double Height => (double)_parameters["h"];
        public double RoundRadius => (double)_parameters["round_r"];
        public double RoundHeight => _parameters.TryGetValue("round_h", out object? value) ? (double)value : 0.001;
        public double? Radius1 => _parameters.TryGetValue("r1", out object? value) ? (double)value : null;
        public double? Radius2 => _parameters.TryGetValue("r2", out object? value) ? (double)value : null;
        public double Resolution => _parameters.TryGetValue("resolution", out object? value) ? (double)value : 200;

        private Cylinder AdjustedCylinder => new(new Dictionary<string, object>
        {
            { "r", Math.Max(0, Radius - RoundRadius) },
            { "h", Height },
            { "r1", (object?)Radius1 ?? 0.0 },
            { "r2", (object?)Radius2 ?? 0.0 },
            { "resolution", Resolution }
        });

        private Cylinder RoundingCylinder => new(new Dictionary<string, object>
        {
            { "r", RoundRadius },
            { "h", RoundHeight },
            { "resolution", Resolution }
        });

        public string OSCADMethod => new Minkowski(AdjustedCylinder, RoundingCylinder).OSCADMethod;

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "RoundedCylinder" },
            { "r", Radius },
            { "h", Height },
            { "round_r", RoundRadius },
            { "round_h", RoundHeight },
            { "r1", (object?)Radius1 ?? 0.0 },
            { "r2", (object?)Radius2 ?? 0.0 },
            { "resolution", Resolution }
        };

        // Client-side example:
        /*
        var roundedCylParams = new Dictionary<string, object>
        {
            { "r", 5.0 }, { "h", 10.0 }, { "round_r", 1.0 }, { "resolution", 150.0 }
        };
        var roundedCylinder = OScad3D.RoundedCylinder.ToScadObject(roundedCylParams);
        Console.WriteLine(roundedCylinder.OSCADMethod); // minkowski() { cylinder(h=10, r=4, $fn=150); cylinder(r=1, h=0.001, $fn=150); };
        var dbData = roundedCylinder.ToDbDictionary(); // { "type": "RoundedCylinder", "r": 5, "h": 10, "round_r": 1, "round_h": 0.001, "r1": null, "r2": null, "resolution": 150 }
        // SQLite: INSERT INTO Models (Type, Radius, Height, RoundRadius, RoundHeight, Radius1, Radius2, Resolution) VALUES ('RoundedCylinder', 5, 10, 1, 0.001, NULL, NULL, 150);
        */
    }
}
