using System.Reflection;
using System.Text.Json;
using Azure.Data.Tables.Poco.Abstractions;

namespace Azure.Data.Tables.Poco.Converters;

internal class StorablePropertyJsonConverter : IStorablePropertyConverter
{
    public JsonSerializerOptions? SerializerOptions { get; set; }

    public bool IsKeyCompliant => true;

    public bool CanConvert(PropertyInfo propertyInfo)
    {
        return true;
    }

    public object? ConvertTo(PropertyInfo propertyInfo, object instance)
    {
        var value = propertyInfo.GetValue(instance);

        return value == null ? null : JsonSerializer.Serialize(value, SerializerOptions);
    }

    public object? ConvertFrom(PropertyInfo propertyInfo, object? value)
    {
        return value == null
            ? null
            : JsonSerializer.Deserialize((string)value, propertyInfo.PropertyType, SerializerOptions);
    }
}