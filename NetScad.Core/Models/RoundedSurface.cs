using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public partial class RoundedSurface(string file, double round_r, bool center = false, bool invert = false, int convexity = 1, double scaleX = 1, double scaleY = 1, double scaleZ = 1, double round_h = 0.0, int resolution = 180) : IScadObject, IDbSerializable
    {
        private readonly string _file = file;
        private readonly double _roundRadius = round_r;
        private readonly bool _center = center;
        private readonly double _roundHeight = round_h;
        private readonly int _resolution = resolution;
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
        public double RoundRadius => _roundRadius;
        public double RoundHeight => _roundHeight;
        public int Resolution => _resolution;

        private Surface AdjustedSurface => new(file: File, center: Center, invert: Invert, convexity: Convexity, scaleX: ScaleX, scaleY: ScaleY, scaleZ: ScaleZ);

        private Cylinder RoundingCylinder => new(new Dictionary<string, object>
        {
            { "r", RoundRadius },
            { "h", RoundHeight },
            { "resolution", Resolution }
        });

        public string OSCADMethod => new Minkowski(AdjustedSurface, RoundingCylinder).OSCADMethod;

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "RoundedSurface" },
            { "file", File },
            { "round_r", RoundRadius },
            { "center", Center },
            { "round_h", RoundHeight },
            { "resolution", Resolution }
        };

        // Client-side example:
        /*
        var roundedSurfParams = new Dictionary<string, object>
        {
            { "file", "heightmap.dat" }, { "round_r", 1.0 }, { "center", true }, { "round_h", 0.002 }, { "resolution", 200.0 }
        };
        var roundedSurface = OScad3D.RoundedSurface.ToScadObject(roundedSurfParams);
        Console.WriteLine(roundedSurface.OSCADMethod); // minkowski() { surface(file="heightmap.dat", center=true, convexity=1); cylinder(r=1, h=0.002, $fn=200); };
        var dbData = roundedSurface.ToDbDictionary(); // { "type": "RoundedSurface", "file": "heightmap.dat", "round_r": 1, "center": true, "round_h": 0.002, "resolution": 200 }
        // SQLite: INSERT INTO Models (Type, File, RoundRadius, Center, RoundHeight, Resolution) VALUES ('RoundedSurface', 'heightmap.dat', 1, 1, 0.002, 200);
        */
    }
}
