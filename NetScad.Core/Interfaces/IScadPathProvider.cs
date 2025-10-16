namespace NetScad.Core.Interfaces
{
    // Define an interface for the .scad path
    public interface IScadPathProvider
    {
        string ScadPath { get; }
    }

    // Implementation
    public class ScadPathProvider : IScadPathProvider
    {
        public string ScadPath { get; }

        public ScadPathProvider(string scadPath)
        {
            ScadPath = scadPath;
        }
    }
}
