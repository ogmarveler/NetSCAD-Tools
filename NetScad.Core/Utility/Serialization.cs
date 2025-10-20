using System.Text.Json.Serialization;

namespace NetScad.Core.Utility
{
    [JsonSerializable(typeof(Dictionary<string, object>))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(double?))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(int))]
    public partial class NetScadJsonSerializerContext : JsonSerializerContext
    {

    }
}
