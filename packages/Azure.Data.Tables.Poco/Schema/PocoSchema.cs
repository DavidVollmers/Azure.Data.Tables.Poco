using System.Reflection;

namespace Azure.Data.Tables.Poco.Schema;

public sealed class PocoSchema
{
    public PocoSchemaProperty PartitionKey { get; }

    public PocoSchemaProperty RowKey { get; }

    public PocoSchemaProperty[] GetProperties { get; }

    public PocoSchemaProperty[] SetProperties { get; }

    public PocoSchemaConstructor Constructor { get; }

    private PocoSchema(PocoSchemaProperty partitionKey, PocoSchemaProperty rowKey, PocoSchemaProperty[] getProperties,
        PocoSchemaProperty[] setProperties, PocoSchemaConstructor constructor)
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
        GetProperties = getProperties;
        SetProperties = setProperties;
        Constructor = constructor;
    }

    public static PocoSchema CreateFromType(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(PocoSchemaProperty.CreateFromPropertyInfo).ToArray();

        var partitionKeys = properties.Where(p => p.IsPartitionKey).ToArray();
        switch (partitionKeys.Length)
        {
            case 0:
                throw new InvalidOperationException(
                    "No partition key specified. Please use the PartitionKeyAttribute.");
            case > 1:
                throw new InvalidOperationException("More than one partition key specified.");
        }

        var rowKeys = properties.Where(p => p.IsRowKey).ToArray();
        switch (rowKeys.Length)
        {
            case 0:
                throw new InvalidOperationException(
                    "No row key specified. Please use the RowKeyAttribute.");
            case > 1:
                throw new InvalidOperationException("More than one row key specified.");
        }

        var getProperties = new List<PocoSchemaProperty>();
        var setProperties = new List<PocoSchemaProperty>();

        foreach (var property in properties)
        {
            if (property.CanRead) getProperties.Add(property);
            else continue;

            if (property.CanWrite) setProperties.Add(property);
        }

        var constructor = PocoSchemaConstructor.CreateFromType(type, getProperties, setProperties);

        return new PocoSchema(partitionKeys[0], rowKeys[0], getProperties.ToArray(), setProperties.ToArray(),
            constructor);
    }
}
