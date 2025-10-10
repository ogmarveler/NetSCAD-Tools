using NetScad.Core.Interfaces;
using System.Collections.Generic;

namespace NetScad.Core.Models
{
    public partial class Surface : IScadObject, IDbSerializable
    {
        private readonly string _file;
        private readonly bool _center;
        private readonly int _convexity;

        public Surface(string file, bool center = false, int convexity = 1)
        {
            _file = file;
            _center = center;
            _convexity = convexity;
        }

        public string File => _file;
        public bool Center => _center;
        public int Convexity => _convexity;

        public string OSCADMethod => $"surface(file = \"{File}\"{(Center ? $", center = {Center.ToString().ToLower()}" : "")}, convexity = {Convexity});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Surface" },
            { "file", File },
            { "center", Center },
            { "convexity", Convexity }
        };

        // Client-side example:
        /*
        var surfParams = new Dictionary<string, object> { { "file", "heightmap.dat" }, { "center", true }, { "convexity", 1 } };
        var surface = OScad3D.Surface.ToScadObject(surfParams);
        Console.WriteLine(surface.OSCADMethod); // surface(file="heightmap.dat", center=true, convexity=1);
        var dbData = surface.ToDbDictionary(); // { "type": "Surface", "file": "heightmap.dat", "center": true, "convexity": 1 }
        // SQLite: INSERT INTO Models (Type, File, Center, Convexity) VALUES ('Surface', 'heightmap.dat', 1, 1);
        */
    }
}
