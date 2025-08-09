namespace Azure.Data.Tables.Poco.Tests.Pocos;

public class DateTimePoco
{
    [PartitionKey, RowKey] [StoreAsString] public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime JustADateTime { get; set; } = DateTime.UtcNow;
}
