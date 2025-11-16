using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public partial class Surface(string file, bool center = false, bool invert = false, int convexity = 1, double scaleX = 1, double scaleY = 1, double scaleZ = 1 ) : IScadObject, IDbSerializable
    {
        private readonly string _file = file;
        private readonly bool _center = center;
        private readonly bool _invert = invert;
        private readonly int _convexity = convexity;
        private readonly double _scaleX = scaleX;
        private readonly double _scaleY = scaleY;
        private readonly double _scaleZ = scaleZ;

        public double ScaleX => _scaleX;
        public double ScaleY => _scaleY;
        public double ScaleZ => _scaleZ;
        public string File => _file;
        public bool Center => _center;
        public bool Invert => _invert;
        public int Convexity => _convexity;

        public string OSCADMethod => $"scale ([{ScaleX},{ScaleY},{ScaleZ}]) surface (file = \"{File}\"{(Center ? $", center = {Center.ToString().ToLower()}" : "")}, invert = {Invert}, convexity = {Convexity});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Surface" },
            { "scaleX", ScaleX },
            { "scaleY", ScaleY },
            { "scaleZ", ScaleZ },
            { "file", File },
            { "center", Center },
            { "invert", Invert },
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
