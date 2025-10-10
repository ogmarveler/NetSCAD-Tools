using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetScad.Core.Interfaces
{
    public interface IDbSerializable
    {
        Dictionary<string, object> ToDbDictionary();
    }
}
