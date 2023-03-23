using System.Text.Json;
using Azure.Data.Tables.Poco.Converters;

namespace Azure.Data.Tables.Poco;

public sealed class StoreAsJsonAttribute : StoreAsAttribute
{
    public JsonSerializerOptions? SerializerOptions
    {
        get => ((StorablePropertyJsonConverter)Converter).SerializerOptions;
        set => ((StorablePropertyJsonConverter)Converter).SerializerOptions = value;
    }

    public StoreAsJsonAttribute() : base(new StorablePropertyJsonConverter())
    {
    }
}