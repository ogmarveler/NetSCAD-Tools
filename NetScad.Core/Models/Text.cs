using NetScad.Core.Interfaces;
using System.Collections.Generic;

namespace NetScad.Core.Models
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
                { "font", font },
                { "halign", halign },
                { "valign", valign },
                { "spacing", spacing },
                { "direction", direction },
                { "language", language },
                { "script", script },
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
            { "font", Font },
            { "halign", HAlign },
            { "valign", VAlign },
            { "spacing", Spacing },
            { "direction", Direction },
            { "language", Language },
            { "script", Script },
            { "resolution", Resolution }
        };

        // Client-side example:
        /*
        var textParams = new Dictionary<string, object> { { "text", "Hello" }, { "size", 12.0 }, { "font", "Arial" }, { "halign", "center" } };
        var text = OScadSpecial.Text.ToScadObject(textParams);
        Console.WriteLine(text.OSCADMethod); // text(text="Hello", size=12, font="Arial", halign="center", ...);
        var dbData = text.ToDbDictionary(); // { "type": "Text", "text": "Hello", "size": 12, "font": "Arial", ... }
        // SQLite: INSERT INTO Models (Type, Text, Size, Font, HAlign, VAlign, Spacing, Direction, Language, Script, Resolution) VALUES ('Text', 'Hello', 12, 'Arial', 'center', 'baseline', 1, 'ltr', NULL, NULL, 200);
        */
    }
}
