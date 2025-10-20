namespace NetScad.Core.Interfaces
{
    // Define an interface for the .scad path
    public interface IScadPathProvider
    {
        string ScadPath { get; }
    }

    // Implementation
    public class ScadPathProvider(string scadPath) : IScadPathProvider
    {
        public string ScadPath { get; } = scadPath;
    }
}
