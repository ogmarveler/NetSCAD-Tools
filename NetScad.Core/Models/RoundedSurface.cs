using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
{
    public partial class RoundedSurface(string file, double round_r, bool center = false, double round_h = 0.001, double resolution = 200) : IScadObject, IDbSerializable
    {
        private readonly string _file = file;
        private readonly double _roundRadius = round_r;
        private readonly bool _center = center;
        private readonly double _roundHeight = round_h;
        private readonly double _resolution = resolution;

        public string File => _file;
        public double RoundRadius => _roundRadius;
        public bool Center => _center;
        public double RoundHeight => _roundHeight;
        public double Resolution => _resolution;

        private Surface AdjustedSurface => new(File, Center, 1);

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
