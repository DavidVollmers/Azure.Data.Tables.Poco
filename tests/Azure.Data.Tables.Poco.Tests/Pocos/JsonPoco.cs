using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Data.Tables.Poco.Tests.Pocos;

public class JsonPoco
{
    [PartitionKey] [RowKey] public string Key { get; set; } = null!;

    [StoreAsJson] public Json Json1 { get; set; } = null!;

    [StoreAsJson]
    [JsonConverter(typeof(CustomJsonConverter))]
    public Json Json2 { get; set; } = null!;

    public class Json
    {
        public string StringProperty { get; set; } = null!;
    }

    public class CustomJsonConverter : JsonConverter<Json>
    {
        public override Json Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new Json { StringProperty = nameof(CustomJsonConverter) };
        }

        public override void Write(Utf8JsonWriter writer, Json value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.StringProperty);
        }
    }
}
