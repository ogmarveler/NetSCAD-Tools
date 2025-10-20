namespace NetScad.Core.Interfaces
{
    public interface IDbSerializable
    {
        Dictionary<string, object> ToDbDictionary();
    }
}
