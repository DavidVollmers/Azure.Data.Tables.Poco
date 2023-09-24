using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Data.Tables.Poco.Abstractions;

namespace Azure.Data.Tables.Poco.Converters;

internal class StorablePropertyJsonConverter : IStorablePropertyConverter
{
    public bool IsKeyCompliant => true;

    public bool CanConvert(PropertyInfo propertyInfo)
    {
        return true;
    }

    public object? ConvertTo(PropertyInfo propertyInfo, object instance)
    {
        var value = propertyInfo.GetValue(instance);
        
        var serializerOptions = GetJsonSerializerOptions(propertyInfo);
        
        return value == null ? null : JsonSerializer.Serialize(value, serializerOptions);
    }

    public object? ConvertFrom(PropertyInfo propertyInfo, object? value)
    {
        var serializerOptions = GetJsonSerializerOptions(propertyInfo);
        
        return value == null
            ? null
            : JsonSerializer.Deserialize((string)value, propertyInfo.PropertyType, serializerOptions);
    }
    
    private static JsonSerializerOptions GetJsonSerializerOptions(MemberInfo propertyInfo)
    {
        var serializerOptions = new JsonSerializerOptions();

        var jsonConverterAttribute = propertyInfo.GetCustomAttribute<JsonConverterAttribute>();
        if (jsonConverterAttribute?.ConverterType != null)
        {
            serializerOptions.Converters.Add(
                (JsonConverter)Activator.CreateInstance(jsonConverterAttribute.ConverterType)!);
        }

        return serializerOptions;
    }
}
