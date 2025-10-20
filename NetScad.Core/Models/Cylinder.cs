using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public partial class Cylinder(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double Radius => (double)_parameters["r"];
        public double Height => (double)_parameters["h"];
        public double? Radius1 => _parameters.TryGetValue("r1", out object? value) ? (double)value : null;
        public double? Radius2 => _parameters.TryGetValue("r2", out object? value) ? (double)value : null;
        public int Resolution => _parameters.TryGetValue("resolution", out object? value) ? (int)value : 100;

        public string OSCADMethod => $"cylinder(h = {Height}, {(Radius1 == null && Radius2 == null ? $"r = {Radius}" : $"r1 = {Radius1 ?? Radius}, r2 = {Radius2 ?? Radius}")}, $fn = {Resolution});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Cylinder" },
            { "r", Radius },
            { "h", Height },
            { "r1", Radius1 ?? 0 },
            { "r2", Radius2 ?? 0 },
            { "resolution", Resolution }
        };

        // Client-side example:
        /*  
        var cylParams = new Dictionary<string, object> { { "r", 5.0 }, { "h", 10.0 }, { "resolution", 50.0 } };
        var cylinder = OScad3D.Cylinder.ToScadObject(cylParams);
        Console.WriteLine(cylinder.OSCADMethod); // cylinder(h=10, r=5, $fn=50);
        var dbData = cylinder.ToDbDictionary(); // { "type": "Cylinder", "r": 5, "h": 10, "r1": null, "r2": null, "resolution": 50 }
        // SQLite: INSERT INTO Models (Type, Radius, Height, Radius1, Radius2, Resolution) VALUES ('Cylinder', 5, 10, null, null, 50);
        */
    }
}
