using Azure.Data.Tables.Poco.Schema;

namespace Azure.Data.Tables.Poco;

public sealed class TableEntityConverter<T> where T : class
{
    public PocoSchema Schema { get; } = PocoSchema.CreateFromType(typeof(T));

    public TableEntity ConvertToEntity(T poco, bool keysOnly = false)
    {
        var entity = new TableEntity
        {
            PartitionKey = (string)Schema.PartitionKey.GetValue(poco)!,
            RowKey = (string)Schema.RowKey.GetValue(poco)!
        };

        if (keysOnly) return entity;

        foreach (var property in Schema.GetProperties)
        {
            if (property.ShouldBeIgnored) continue;

            if (property.Name == nameof(ITableEntity.Timestamp)) continue;

            entity[property.Name] = property.GetValue(poco);
        }

        return entity;
    }

    public T ConvertFromEntity(TableEntity entity)
    {
        var poco = (T)Schema.Constructor.CreateInstance(entity);

        foreach (var property in Schema.SetProperties)
        {
            if (property is { ShouldBeIgnored: true, IsPartitionKey: false, IsRowKey: false }) continue;

            if (property.Name == nameof(ITableEntity.Timestamp))
            {
                property.SetValue(poco, entity.Timestamp);
                continue;
            }

            if (!entity.ContainsKey(property.Name)) continue;

            property.SetValue(poco, entity[property.Name]);
        }

        return poco;
    }
}
