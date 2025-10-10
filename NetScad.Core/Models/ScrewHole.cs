using NetScad.Core.Interfaces;
using System.Collections.Generic;

namespace NetScad.Core.Models
{
    public partial class ScrewHole : IScadObject, IDbSerializable
    {
        private readonly ScrewSize _screwSize;
        private readonly double _height;
        private readonly double _resolution;

        public ScrewHole(ScrewSize screwSize, double h, double resolution = 100)
        {
            _screwSize = screwSize;
            _height = h;
            _resolution = resolution;
        }

        public ScrewSize ScrewSize => _screwSize;
        public double Height => _height;
        public double Resolution => _resolution;

        private Cylinder HoleCylinder => new Cylinder(new Dictionary<string, object>
        {
            { "r", ScrewSize.ClearanceHoleRadius },
            { "h", Height },
            { "resolution", Resolution }
        });

        public string OSCADMethod => HoleCylinder.OSCADMethod;

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "ScrewHole" },
            { "screw_radius", ScrewSize.ScrewRadius },
            { "screw_head_radius", ScrewSize.ScrewHeadRadius },
            { "threaded_insert_radius", ScrewSize.ThreadedInsertRadius },
            { "clearance_hole_radius", ScrewSize.ClearanceHoleRadius },
            { "countersunk_height", ScrewSize.CountersunkHeight },
            { "h", Height },
            { "resolution", Resolution }
        };

        // Client-side example:
        /*
        var screwHoleParams = new Dictionary<string, object> { { "screwSize", ScrewSizes.M3 }, { "h", 10.0 }, { "resolution", 100.0 } };
        var screwHole = ScrewPrimitives.ScrewHole.ToScadObject(screwHoleParams);
        Console.WriteLine(screwHole.OSCADMethod); // cylinder(h=10, r=1.75, $fn=100);
        var dbData = screwHole.ToDbDictionary(); // { "type": "ScrewHole", "screw_radius": 1.5, "screw_head_radius": 3, ... }
        // SQLite: INSERT INTO Models (Type, ScrewRadius, ScrewHeadRadius, ThreadedInsertRadius, ClearanceHoleRadius, CountersunkHeight, Height, Resolution) VALUES ('ScrewHole', 1.5, 3, 1.8, 1.75, 1.8, 10, 100);
        */
    }
}
