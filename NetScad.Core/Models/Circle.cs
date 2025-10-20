using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public partial class Circle(Dictionary<string, object> parameters) : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters = parameters;

        public double Radius => (double)_parameters["r"];
        public double Resolution => _parameters.TryGetValue("resolution", out object? value) ? (double)value : 100;

        public string OSCADMethod => $"circle(r = {Radius}, $fn = {Resolution});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Circle" },
            { "r", Radius },
            { "resolution", Resolution }
        };

        // Client-side example:
        /*
        var circleParams = new Dictionary<string, object> { { "r", 5.0 }, { "resolution", 100.0 } };
        var circle = OScad2D.Circle.ToScadObject(circleParams);
        Console.WriteLine(circle.OSCADMethod); // circle(r=5, $fn=100);
        var dbData = circle.ToDbDictionary(); // { "type": "Circle", "r": 5, "resolution": 100 }
        // SQLite: INSERT INTO Models (Type, Radius, Resolution) VALUES ('Circle', 5, 100);
        */
    }
}
