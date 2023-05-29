using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Azure.Data.Tables.Models;

namespace Azure.Data.Tables.Poco;

public class TypedTableClient<T> where T : class
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly TableClient _tableClient;
    private readonly TableEntityConverter<T> _tableEntityConverter;

    public string Name { get; }

    internal TypedTableClient(TableServiceClient tableServiceClient)
        : this(tableServiceClient, new TableEntityConverter<T>(), GetTableName())
    {
    }

    private TypedTableClient(TableServiceClient tableServiceClient, TableEntityConverter<T> tableEntityConverter,
        string name)
    {
        _tableServiceClient = tableServiceClient ?? throw new ArgumentNullException(nameof(tableServiceClient));
        Name = name ?? throw new ArgumentNullException(nameof(name));

        if (!new Regex("^[A-Za-z][A-Za-z0-9]{2,62}$").IsMatch(Name))
            throw new InvalidOperationException(
                "Table name is not compliant to 'https://learn.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model#table-names'.");

        _tableClient = _tableServiceClient.GetTableClient(Name);
        _tableEntityConverter = tableEntityConverter;
    }

    public async Task<Response> AddAsync(T poco, CancellationToken cancellationToken = default)
    {
        if (poco == null) throw new ArgumentNullException(nameof(poco));

        var entity = _tableEntityConverter.ConvertToEntity(poco);

        return await _tableClient.AddEntityAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Response<TableItem>> CreateTableAsync(CancellationToken cancellationToken = default)
    {
        return await _tableClient.CreateAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Response<TableItem>> CreateTableIfNotExistsAsync(CancellationToken cancellationToken = default)
    {
        return await _tableClient.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Response> DeleteAsync(T poco, ETag ifMatch = default,
        CancellationToken cancellationToken = default)
    {
        if (poco == null) throw new ArgumentNullException(nameof(poco));

        var entity = _tableEntityConverter.ConvertToEntity(poco, true);

        return await _tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, ifMatch, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Response> DeleteTableAsync(CancellationToken cancellationToken = default)
    {
        return await _tableClient.DeleteAsync(cancellationToken).ConfigureAwait(false);
    }

    //TODO don't omit Response
    public async Task<T> GetAsync(string partitionKey, string rowKey, IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));
        if (rowKey == null) throw new ArgumentNullException(nameof(rowKey));

        var response = await _tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey, select, cancellationToken)
            .ConfigureAwait(false);

        return _tableEntityConverter.ConvertFromEntity(response.Value);
    }

    //TODO don't omit Response
    public async Task<T?> GetIfExistsAsync(string partitionKey, string rowKey, IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));
        if (rowKey == null) throw new ArgumentNullException(nameof(rowKey));

        var response = await _tableClient
            .GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey, select, cancellationToken)
            .ConfigureAwait(false);

        return !response.HasValue ? null : _tableEntityConverter.ConvertFromEntity(response.Value);
    }

    public async IAsyncEnumerable<T> QueryAsync(string? filter = null, int? maxPerPage = null,
        IEnumerable<string>? select = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var asyncPageable = _tableClient.QueryAsync<TableEntity>(filter, maxPerPage, select, cancellationToken);

        await foreach (var entity in asyncPageable.ConfigureAwait(false))
        {
            var poco = _tableEntityConverter.ConvertFromEntity(entity);

            yield return poco;
        }
    }

    public TypedTableClient<T> OverrideTableName(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        return new TypedTableClient<T>(_tableServiceClient, _tableEntityConverter, name);
    }

    //TODO transactions

    public async Task<Response> UpdateAsync(T poco, ETag ifMatch = default,
        TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default)
    {
        if (poco == null) throw new ArgumentNullException(nameof(poco));

        if (ifMatch == default) ifMatch = new ETag("*");

        var entity = _tableEntityConverter.ConvertToEntity(poco);

        return await _tableClient.UpdateEntityAsync(entity, ifMatch, mode, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Response> UpsertAsync(T poco, TableUpdateMode mode = TableUpdateMode.Merge,
        CancellationToken cancellationToken = default)
    {
        if (poco == null) throw new ArgumentNullException(nameof(poco));

        var entity = _tableEntityConverter.ConvertToEntity(poco);

        return await _tableClient.UpsertEntityAsync(entity, mode, cancellationToken).ConfigureAwait(false);
    }

    public static string CreateQueryFilter(Expression<Func<T, bool>> filterExpression)
    {
        if (filterExpression == null) throw new ArgumentNullException(nameof(filterExpression));

        return TableClient.CreateQueryFilter(filterExpression);
    }

    private static string GetTableName()
    {
        var type = typeof(T);

        var storableAttribute = type.GetCustomAttribute<StorableAttribute>(true);

        //TODO sanitize generic type names
        return storableAttribute?.Name ?? type.Name;
    }
}