using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class ForLoop(string loopExpression, IScadObject[] body) : IScadObject, IDbSerializable
    {
        private readonly string _loopExpression = loopExpression;
        private readonly IScadObject[] _body = body;

        public string LoopExpression => _loopExpression;
        public IScadObject[] Body => _body;

        public string OSCADMethod => $"for({LoopExpression}) {{ {string.Join("\n", Body.Select(c => c.OSCADMethod))} }};";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "ForLoop" },
            { "loopExpression", LoopExpression }
        };
    }
}
