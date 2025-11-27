using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Multmatrix(List<List<double>> matrix, IScadObject[] children) : IScadObject, IDbSerializable
    {
        private readonly List<List<double>> _matrix = matrix;
        private readonly IScadObject[] _children = children;

        public List<List<double>> Matrix => _matrix;
        public IScadObject[] Children => _children;

        public string OSCADMethod => $"multmatrix([{string.Join(", ", Matrix.Select(row => $"[{string.Join(", ", row)}]"))}]) {{ {string.Join("\n", Children.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Multmatrix" }
        };
    }
}
