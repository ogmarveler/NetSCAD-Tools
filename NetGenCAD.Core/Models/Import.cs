using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Import(string file, int convexity = 1) : IScadObject, IDbSerializable
    {
        private readonly string _file = file;
        private readonly int _convexity = convexity;

        public string File => _file;
        public int Convexity => _convexity;

        public string OSCADMethod => $"import(\"{File}\", convexity = {Convexity});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Import" },
            { "file", File },
            { "convexity", Convexity }
        };
    }
}
