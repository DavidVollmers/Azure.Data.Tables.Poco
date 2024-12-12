using Azure.Data.Tables.Models;

namespace Azure.Data.Tables.Poco;

public interface ITypedTableClient<T> where T : class
{
    string Name { get; }

    Task<Response> AddAsync(T poco, CancellationToken cancellationToken = default);

    Task<Response<TableItem>> CreateTableAsync(CancellationToken cancellationToken = default);

    Task<Response<TableItem>> CreateTableIfNotExistsAsync(CancellationToken cancellationToken = default);

    Task<Response> DeleteAsync(T poco, ETag ifMatch = default, CancellationToken cancellationToken = default);

    Task<Response> DeleteTableAsync(CancellationToken cancellationToken = default);

    Task<T> GetAsync(string partitionKey, string rowKey, IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default);

    Task<T?> GetIfExistsAsync(string partitionKey, string rowKey, IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default);

    AsyncPageable<T> QueryAsync(string? filter = null, int? maxPerPage = null,
        IEnumerable<string>? select = null, CancellationToken cancellationToken = default);

    TypedTableClient<T> OverrideTableName(string name);

    Task<Response> UpdateAsync(T poco, ETag ifMatch = default,
        TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default);

    Task<Response> UpsertAsync(T poco, TableUpdateMode mode = TableUpdateMode.Merge,
        CancellationToken cancellationToken = default);
}
