using NetGenCAD.Core.Interfaces;

namespace NetGenCAD.Core.Models
{
    public partial class Text : IScadObject, IDbSerializable
    {
        private readonly Dictionary<string, object> _parameters;

        public Text(string text, double size = 10, string? font = null, string halign = "left", string valign = "baseline", double spacing = 1, string direction = "ltr", string? language = null, string? script = null, double resolution = 200)
        {
            _parameters = new Dictionary<string, object>
            {
                { "text", text },
                { "size", size },
                { "font", (object?)font ?? string.Empty },
                { "halign", halign },
                { "valign", valign },
                { "spacing", spacing },
                { "direction", direction },
                { "language", (object?)language ?? string.Empty },
                { "script", (object?)script ?? string.Empty },
                { "resolution", resolution }
            };
        }

        public string TextValue => (string)_parameters["text"];
        public double Size => (double)_parameters["size"];
        public string? Font => (string?)_parameters["font"];
        public string HAlign => (string)_parameters["halign"];
        public string VAlign => (string)_parameters["valign"];
        public double Spacing => (double)_parameters["spacing"];
        public string Direction => (string)_parameters["direction"];
        public string? Language => (string?)_parameters["language"];
        public string? Script => (string?)_parameters["script"];
        public double Resolution => (double)_parameters["resolution"];

        public string OSCADMethod => $"text(text = \"{TextValue}\", size = {Size}{(Font != null ? $", font = \"{Font}\"" : "")}, halign = \"{HAlign}\", valign = \"{VAlign}\", spacing = {Spacing}, direction = \"{Direction}\"{(Language != null ? $", language = \"{Language}\"" : "")}{(Script != null ? $", script = \"{Script}\"" : "")}, $fn = {Resolution});";

        public Dictionary<string, object> ToDbDictionary() => new()
        {
            { "type", "Text" },
            { "text", TextValue },
            { "size", Size },
            { "font", (object?)Font ?? string.Empty },
            { "halign", HAlign },
            { "valign", VAlign },
            { "spacing", Spacing },
            { "direction", Direction },
            { "language", (object?)Language ?? string.Empty },
            { "script", (object?)Script ?? string.Empty },
            { "resolution", Resolution }
        };
    }
}
