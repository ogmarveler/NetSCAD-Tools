using NetScad.Core.Interfaces;
using System.Collections.Generic;

namespace NetScad.Core.Models
{
    public partial class Square : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters;

        public Square(Dictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public double SizeX => (double)_parameters["size_x"];
        public double SizeY => (double)_parameters["size_y"];
        public bool Center => _parameters.ContainsKey("center") ? (bool)_parameters["center"] : false;

        public string OSCADMethod => $"square([{SizeX}, {SizeY}]{(Center ? $", center = {Center.ToString().ToLower()}" : "")});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Square" },
            { "size_x", SizeX },
            { "size_y", SizeY },
            { "center", Center }
        };

        // Client-side example:
        /*
        var squareParams = new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "center", true } };
        var square = OScad2D.Square.ToScadObject(squareParams);
        Console.WriteLine(square.OSCADMethod); // square([10, 20], center=true);
        var dbData = square.ToDbDictionary(); // { "type": "Square", "size_x": 10, "size_y": 20, "center": true }
        // SQLite: INSERT INTO Models (Type, SizeX, SizeY, Center) VALUES ('Square', 10, 20, 1);
        */
    }
}
