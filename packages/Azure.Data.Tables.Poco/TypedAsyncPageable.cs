namespace Azure.Data.Tables.Poco;

internal sealed class TypedAsyncPageable<T> : AsyncPageable<T> where T : class
{
    private readonly AsyncPageable<TableEntity> _pageable;
    private readonly TableEntityConverter<T> _tableEntityConverter;

    public TypedAsyncPageable(AsyncPageable<TableEntity> pageable, TableEntityConverter<T> tableEntityConverter)
    {
        _pageable = pageable;
        _tableEntityConverter = tableEntityConverter;
    }

    public override async IAsyncEnumerable<Page<T>> AsPages(string? continuationToken = null, int? pageSizeHint = null)
    {
        var pages = _pageable.AsPages(continuationToken, pageSizeHint);
        await foreach (var page in pages)
        {
            yield return Page<T>.FromValues(
                page.Values.Select(e => _tableEntityConverter.ConvertFromEntity(e)).ToList(), page.ContinuationToken,
                page.GetRawResponse());
        }
    }
}
