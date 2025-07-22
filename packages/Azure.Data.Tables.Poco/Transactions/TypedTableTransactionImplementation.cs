namespace Azure.Data.Tables.Poco.Transactions;

internal sealed class TypedTableTransactionImplementation<T> : ITypedTableTransaction<T> where T : class
{
    private readonly TableClient _tableClient;
    private readonly TableEntityConverter<T> _tableEntityConverter;
    private readonly List<TableTransactionAction> _actions = new();

    public TypedTableTransactionImplementation(TableClient tableClient, TableEntityConverter<T> tableEntityConverter)
    {
        _tableClient = tableClient;
        _tableEntityConverter = tableEntityConverter;
    }

    public Task SubmitAsync(CancellationToken cancellationToken = default)
    {
        if (_actions.Count == 0) throw new InvalidOperationException("No actions to submit.");

        return _tableClient.SubmitTransactionAsync(_actions, cancellationToken);
    }

    public void Add(T poco)
    {
        if (poco == null) throw new ArgumentNullException(nameof(poco));

        var entity = _tableEntityConverter.ConvertToEntity(poco);
        _actions.Add(new TableTransactionAction(TableTransactionActionType.Add, entity));
    }

    public void Update(T poco, ETag ifMatch = default, TableUpdateMode mode = TableUpdateMode.Merge)
    {
        if (poco == null) throw new ArgumentNullException(nameof(poco));

        var entity = _tableEntityConverter.ConvertToEntity(poco, true);
        _actions.Add(new TableTransactionAction(
            mode == TableUpdateMode.Merge
                ? TableTransactionActionType.UpdateMerge
                : TableTransactionActionType.UpdateReplace, entity, ifMatch));
    }

    public void Upsert(T poco, TableUpdateMode mode = TableUpdateMode.Merge)
    {
        if (poco == null) throw new ArgumentNullException(nameof(poco));

        var entity = _tableEntityConverter.ConvertToEntity(poco, true);
        _actions.Add(new TableTransactionAction(
            mode == TableUpdateMode.Merge
                ? TableTransactionActionType.UpsertMerge
                : TableTransactionActionType.UpsertReplace, entity));
    }

    public void Delete(T poco, ETag ifMatch = default)
    {
        if (poco == null) throw new ArgumentNullException(nameof(poco));

        var entity = _tableEntityConverter.ConvertToEntity(poco, true);
        _actions.Add(new TableTransactionAction(TableTransactionActionType.Delete, entity, ifMatch));
    }
}
