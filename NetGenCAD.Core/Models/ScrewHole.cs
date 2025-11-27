using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class ScrewHole(ScrewSize screwSize, double h, double resolution = 100) : IScadObject, IDbSerializable
    {
        private readonly ScrewSize _screwSize = screwSize;
        private readonly double _height = h;
        private readonly double _resolution = resolution;

        public ScrewSize ScrewSize => _screwSize;
        public double Height => _height;
        public double Resolution => _resolution;

        private Cylinder HoleCylinder => new(new Dictionary<string, object>
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
    }
}
