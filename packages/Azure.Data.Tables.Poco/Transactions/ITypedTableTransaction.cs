namespace Azure.Data.Tables.Poco.Transactions;

public interface ITypedTableTransaction
{
    Task SubmitAsync(CancellationToken cancellationToken = default);
}

public interface ITypedTableTransaction<in T> : ITypedTableTransaction where T : class
{
    void Add(T poco);

    void Update(T poco, ETag ifMatch = default, TableUpdateMode mode = TableUpdateMode.Merge);

    void Upsert(T poco, TableUpdateMode mode = TableUpdateMode.Merge);

    void Delete(T poco, ETag ifMatch = default);
}

public interface ITypedTableTransaction<in T1, in T2> : ITypedTableTransaction<T1>
    where T1 : class where T2 : class
{
    void Add(T2 poco);

    void Update(T2 poco, ETag ifMatch = default, TableUpdateMode mode = TableUpdateMode.Merge);

    void Upsert(T2 poco, TableUpdateMode mode = TableUpdateMode.Merge);

    void Delete(T2 poco, ETag ifMatch = default);
}
