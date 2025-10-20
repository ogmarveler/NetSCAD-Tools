using System.Text;

namespace NetScad.Axis.SCAD.Utility
{
    internal static class BlockStatement
    {
        public static string GetCoordinates(Enum scope, List<double> coordinates)
        {
            StringBuilder sb = new();
            sb.Append($"{scope.ToString().ToLower()} ([");
            foreach (var (coordinate, index) in coordinates.Select((v, i) => (v, i)))
            {
                sb.Append(coordinate);

                if (index < coordinates.Count - 1)
                    sb.Append(", ");
            }
            sb.Append("]);");
            return sb.ToString();
        }

        public static string GetIterationHeader(Enum scope, string iterator, List<double> range)
        {
            StringBuilder sb = new();

            sb.Append($"{scope.ToString().ToLower()} ({iterator} = [");
            foreach (var (point, index) in range.Select((v, i) => (v, i)))
            {
                sb.Append(point);

                if (index < range.Count - 1)
                    sb.Append(':');
            }
            sb.Append("])");
            return sb.ToString();
        }

        public static string GetModule(string name, List<string> parameters, string content)
        {
            StringBuilder sb = new();

            sb.Append($"module {name.ToString().ToLower()} (");

            if (parameters is not null && parameters.Count > 0)
            {
                foreach (var (param, index) in parameters.Select((v, i) => (v, i)))
                {
                    sb.Append(param);
                    if (index < parameters.Count - 1)
                        sb.Append(", ");
                }
            }
            sb.Append(") {\n");
            sb.Append($"    {content}\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
