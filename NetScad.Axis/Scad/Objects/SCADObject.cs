using NetScad.Axis.SCAD.Utility;

namespace NetScad.Axis.SCAD.Objects
{
    public static class SCADObject
    {
        public static string Module(string name, string content = "", List<string>? parameters = null)
        {
            return $"{BlockStatement.GetModule(name: name, parameters: parameters ?? [], content: content)}\n";
        }
    }
}
