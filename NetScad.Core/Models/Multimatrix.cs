using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
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

        // Client-side example:
        /*
        var cube = OScad3D.Cube.ToScadObject(new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } });
        var matrixParams = new Dictionary<string, object>
        {
            { "matrix", new List<List<double>> { new() { 1, 0, 0, 0 }, new() { 0, 1, 0, 0 }, new() { 0, 0, 1, 0 }, new() { 0, 0, 0, 1 } } },
            { "children", new IScadObject[] { cube } }
        };
        var multmatrix = OScadTransform.Multmatrix.ToScadObject(matrixParams);
        Console.WriteLine(multmatrix.OSCADMethod); // multmatrix([[1,0,0,0],[0,1,0,0],[0,0,1,0],[0,0,0,1]]) { cube([10, 20, 30]); };
        var dbData = multmatrix.ToDbDictionary(); // { "type": "Multmatrix" }
        // SQLite: INSERT INTO Models (Type) VALUES ('Multmatrix');
        // Store matrix/children in separate Matrix and Children tables
        */
    }
}
