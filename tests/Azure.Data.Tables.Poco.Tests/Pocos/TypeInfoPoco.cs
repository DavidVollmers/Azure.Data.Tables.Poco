namespace Azure.Data.Tables.Poco.Tests.Pocos;

public class TypeInfoPoco
{
    [StoreAsTypeInfo] public Type Type { get; set; } = null!;

    [PartitionKey] public string PartitionKey => Type.Name;

    [RowKey] [StoreAsJson] public object? Instance { get; set; }
}