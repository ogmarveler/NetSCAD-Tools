namespace NetScad.Core.Interfaces
{
    public interface IScadObject : IDbSerializable
    {
        string OSCADMethod { get; }
    }
}