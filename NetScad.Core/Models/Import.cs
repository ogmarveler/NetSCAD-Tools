using NetScad.Core.Interfaces;

namespace NetScad.Core.Models
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

        // Client-side example:
        /*
        var importParams = new Dictionary<string, object> { { "file", "model.stl" }, { "convexity", 2 } };
        var import = OScadSpecial.Import.ToScadObject(importParams);
        Console.WriteLine(import.OSCADMethod); // import("model.stl", convexity=2);
        var dbData = import.ToDbDictionary(); // { "type": "Import", "file": "model.stl", "convexity": 2 }
        // SQLite: INSERT INTO Models (Type, File, Convexity) VALUES ('Import', 'model.stl', 2);
        */
    }
}
