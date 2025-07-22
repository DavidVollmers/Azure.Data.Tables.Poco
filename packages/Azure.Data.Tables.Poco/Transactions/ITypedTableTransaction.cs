namespace Azure.Data.Tables.Poco.Transactions;

public interface ITypedTableTransaction<in T> where T : class
{
    Task SubmitAsync(CancellationToken cancellationToken = default);

    void Add(T poco);

    void Update(T poco, ETag ifMatch = default, TableUpdateMode mode = TableUpdateMode.Merge);

    void Upsert(T poco, TableUpdateMode mode = TableUpdateMode.Merge);

    void Delete(T poco, ETag ifMatch = default);
}
