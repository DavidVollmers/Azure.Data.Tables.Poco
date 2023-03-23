namespace Azure.Data.Tables.Poco.Tests.Pocos;

public class TypeInfoPoco
{
    [PartitionKey] [StoreAsTypeInfo] public Type Type { get; set; } = null!;

    [RowKey] [StoreAsJson] public object? Instance { get; set; }
}