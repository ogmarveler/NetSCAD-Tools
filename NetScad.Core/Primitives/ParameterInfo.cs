using System;

namespace NetScad.Core.Primitives
{
    public class ParameterInfo
    {
        public string Name { get; }
        public Type Type { get; }
        public object? DefaultValue { get; }

        public ParameterInfo(string name, Type type, object? defaultValue = null)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }
    }
}
