using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class ScrewHeadCountersink(ScrewSize screwSize, double resolution = 100) : IScadObject, IDbSerializable
    {
        private readonly ScrewSize _screwSize = screwSize;
        private readonly double _resolution = resolution;

        public ScrewSize ScrewSize => _screwSize;
        public double Resolution => _resolution;

        private Cylinder CountersinkCylinder => new(new Dictionary<string, object>
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
    }
}
