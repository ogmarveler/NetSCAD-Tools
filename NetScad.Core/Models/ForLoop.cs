using NetScad.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace NetScad.Core.Models
{
    public partial class ForLoop : IScadObject, IDbSerializable
    {
        private readonly string _loopExpression;
        private readonly IScadObject[] _body;

        public ForLoop(string loopExpression, IScadObject[] body)
        {
            _loopExpression = loopExpression;
            _body = body;
        }

        public string LoopExpression => _loopExpression;
        public IScadObject[] Body => _body;

        public string OSCADMethod => $"for({LoopExpression}) {{ {string.Join("\n", Body.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "ForLoop" },
            { "loopExpression", LoopExpression }
        };

        // Client-side example:
        /*
        var cube = OScad3D.Cube.ToScadObject(new Dictionary<string, object> { { "size_x", 10.0 }, { "size_y", 20.0 }, { "size_z", 30.0 } });
        var forParams = new Dictionary<string, object> { { "loopExpression", "i=[0:2]" }, { "body", new IScadObject[] { cube } } };
        var forLoop = OScadIteration.For.ToScadObject(forParams);
        Console.WriteLine(forLoop.OSCADMethod); // for(i=[0:2]) { cube([10, 20, 30]); };
        var dbData = forLoop.ToDbDictionary(); // { "type": "ForLoop", "loopExpression": "i=[0:2]" }
        // SQLite: INSERT INTO Models (Type, LoopExpression) VALUES ('ForLoop', 'i=[0:2]');
        // Store body in a separate Body table
        */
    }
}
