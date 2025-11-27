using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Difference(params IScadObject[] children) : IScadObject, IDbSerializable
    {
        private readonly IScadObject[] _children = children;

        public IScadObject[] Children => _children;

        public string OSCADMethod => $"difference() {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Difference" }
        };
    }
}
