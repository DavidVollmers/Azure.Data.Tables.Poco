namespace Azure.Data.Tables.Poco;

public static class TableServiceClientExtensions
{
    public static TypedTableClient<T> GetTableClient<T>(this TableServiceClient tableServiceClient) where T : class
    {
        if (tableServiceClient == null) throw new ArgumentNullException(nameof(tableServiceClient));

        return new TypedTableClient<T>(tableServiceClient);
    }
}