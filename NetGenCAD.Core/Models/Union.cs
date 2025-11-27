using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Union(params IScadObject[] children) : IScadObject, IDbSerializable
    {
        private readonly IScadObject[] _children = children;

        public IScadObject[] Children => _children;

        public string OSCADMethod => $"union() {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Union" }
        };
    }
}