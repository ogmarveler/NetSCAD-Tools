using NetScad.Core.Interfaces;
using System.Collections.Generic;

namespace NetScad.Core.Models
{
    public partial class ScrewHeadCountersink : IScadObject, IDbSerializable
    {
        private readonly ScrewSize _screwSize;
        private readonly double _resolution;

        public ScrewHeadCountersink(ScrewSize screwSize, double resolution = 100)
        {
            _screwSize = screwSize;
            _resolution = resolution;
        }

        public ScrewSize ScrewSize => _screwSize;
        public double Resolution => _resolution;

        private Cylinder CountersinkCylinder => new Cylinder(new Dictionary<string, object>
        {
            { "r", ScrewSize.ScrewHeadRadius * 2 },
            { "h", ScrewSize.CountersunkHeight },
            { "resolution", Resolution }
        });

        public string OSCADMethod => CountersinkCylinder.OSCADMethod;

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "ScrewHeadCountersink" },
            { "screw_radius", ScrewSize.ScrewRadius },
            { "screw_head_radius", ScrewSize.ScrewHeadRadius },
            { "threaded_insert_radius", ScrewSize.ThreadedInsertRadius },
            { "clearance_hole_radius", ScrewSize.ClearanceHoleRadius },
            { "countersunk_height", ScrewSize.CountersunkHeight },
            { "resolution", Resolution }
        };

        // Client-side example:
        /*
        var screwCounterParams = new Dictionary<string, object> { { "screwSize", ScrewSizes.M3 }, { "resolution", 100.0 } };
        var screwCounter = ScrewPrimitives.ScrewHeadCountersink.ToScadObject(screwCounterParams);
        Console.WriteLine(screwCounter.OSCADMethod); // cylinder(h=1.8, r=6, $fn=100);
        var dbData = screwCounter.ToDbDictionary(); // { "type": "ScrewHeadCountersink", "screw_radius": 1.5, "screw_head_radius": 3, ... }
        // SQLite: INSERT INTO Models (Type, ScrewRadius, ScrewHeadRadius, ThreadedInsertRadius, ClearanceHoleRadius, CountersunkHeight, Resolution) VALUES ('ScrewHeadCountersink', 1.5, 3, 1.8, 1.75, 1.8, 100);
        */
    }
}
