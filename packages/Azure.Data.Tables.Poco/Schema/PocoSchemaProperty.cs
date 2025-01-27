using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Azure.Data.Tables.Poco.Schema;

public sealed class PocoSchemaProperty
{
    // https://learn.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model#characters-disallowed-in-key-fields
    private static readonly byte[] DisallowedKeyValueCharacters =
        "/\\#?\t\n\r"u8.ToArray().Concat(new byte[32].Select((_, i) => (byte)(127 + i))).ToArray();

    private readonly PropertyInfo _propertyInfo;

    private readonly StoreAsAttribute? _storeAsAttribute;

    public bool CanRead => _propertyInfo.CanRead;

    public bool CanWrite => _propertyInfo.CanWrite;

    public string Name { get; }

    public bool IsPartitionKey { get; }

    public bool IsRowKey { get; }

    public bool ShouldBeIgnored { get; set; }

    private PocoSchemaProperty(string name, PropertyInfo propertyInfo, StoreAsAttribute? storeAsAttribute,
        bool isPartitionKey, bool isRowKey, bool shouldBeIgnored)
    {
        Name = name;

        _propertyInfo = propertyInfo;
        _storeAsAttribute = storeAsAttribute;

        IsPartitionKey = isPartitionKey;
        IsRowKey = isRowKey;
        ShouldBeIgnored = shouldBeIgnored;
    }

    public object? GetValue(object instance)
    {
        var propertyValue = _propertyInfo.GetValue(instance);

        var value = _storeAsAttribute != null
            ? _storeAsAttribute.Converter.ConvertTo(_propertyInfo, instance)
            : propertyValue;

        if (_propertyInfo.PropertyType.IsEnum && value != null)
        {
            value = (int)value;
        }

        if (!IsPartitionKey && !IsRowKey) return value;

        if (value == null)
            throw new InvalidOperationException(
                $"Partition or row key value cannot be null. Please define a value for property '{_propertyInfo.Name}' of type '{_propertyInfo.DeclaringType!.FullName}'.");

        if (Encoding.UTF8.GetBytes((string)value).Any(b => DisallowedKeyValueCharacters.Contains(b)))
            throw new InvalidOperationException(
                $"Partition or row key contains disallowed characters. Please define a value for property '{_propertyInfo.Name}' of type '{_propertyInfo.DeclaringType!.FullName}' which is compliant to 'https://learn.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model#characters-disallowed-in-key-fields'.");

        return value;
    }

    public void SetValue(object instance, object? value)
    {
        var propertyValue = _storeAsAttribute != null
            ? _storeAsAttribute.Converter.ConvertFrom(_propertyInfo, value)
            : value;

        if (_propertyInfo.PropertyType.IsEnum && propertyValue != null)
        {
            propertyValue = Enum.ToObject(_propertyInfo.PropertyType, (int)propertyValue);
        }

        _propertyInfo.SetValue(instance, propertyValue);
    }

    public static PocoSchemaProperty CreateFromPropertyInfo(PropertyInfo propertyInfo)
    {
        var isPartitionKey = propertyInfo.GetCustomAttribute<PartitionKeyAttribute>(true) != null;
        var isRowKey = propertyInfo.GetCustomAttribute<RowKeyAttribute>(true) != null;

        var storableAttribute = propertyInfo.GetCustomAttribute<StorableAttribute>();
        var name = storableAttribute?.Name ?? propertyInfo.Name;
        if (name == nameof(ITableEntity.PartitionKey) && !isPartitionKey ||
            name == nameof(ITableEntity.RowKey) && !isRowKey)
        {
            throw new InvalidOperationException(
                $"Property '{propertyInfo.Name}' of type '{propertyInfo.DeclaringType!.FullName}' is named as partition or row key but is not specified as such. Please use the PartitionKeyAttribute or RowKeyAttribute. ");
        }

        if (isPartitionKey) name = nameof(ITableEntity.PartitionKey);
        else if (isRowKey) name = nameof(ITableEntity.RowKey);

        var storeAsAttribute = propertyInfo.GetCustomAttribute<StoreAsAttribute>(true);
        if (storeAsAttribute != null && !storeAsAttribute.Converter.CanConvert(propertyInfo))
        {
            throw new InvalidOperationException(
                $"Property '{propertyInfo.Name}' of type '{propertyInfo.DeclaringType!.FullName}' cannot be converted using the specified StoreAsAttribute.");
        }

        if ((isPartitionKey || isRowKey) &&
            storeAsAttribute?.Converter.IsKeyCompliant != true && propertyInfo.PropertyType != typeof(string))
        {
            throw new InvalidOperationException(
                $"Property '{propertyInfo.Name}' of type '{propertyInfo.DeclaringType!.FullName}' cannot be used as a partition or row key. Either define the property as type '{typeof(string).FullName}' or use a key compliant StoreAsAttribute.");
        }

        var shouldBeIgnored = propertyInfo.GetCustomAttribute<IgnoreDataMemberAttribute>(true) != null ||
                              propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>(true) != null ||
                              isPartitionKey || isRowKey;

        return new PocoSchemaProperty(name, propertyInfo, storeAsAttribute, isPartitionKey, isRowKey, shouldBeIgnored);
    }
}
