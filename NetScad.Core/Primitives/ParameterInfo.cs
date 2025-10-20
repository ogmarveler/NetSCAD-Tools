namespace NetScad.Core.Primitives
{
    public class ParameterInfo(string name, Type type, object? defaultValue = null)
    {
        public string Name { get; } = name;
        public Type Type { get; } = type;
        public object? DefaultValue { get; } = defaultValue;
    }
}
