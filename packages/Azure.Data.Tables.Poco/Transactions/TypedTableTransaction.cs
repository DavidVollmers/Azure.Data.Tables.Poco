namespace Azure.Data.Tables.Poco.Transactions;

public sealed class TypedTableTransaction
{
    public static ITypedTableTransaction<T> For<T>(ITypedTableClient<T> tableClient)
        where T : class
    {
    }

    public static ITypedTableTransaction<T1, T2> For<T1, T2>(ITypedTableClient<T1> t1, ITypedTableClient<T2> t2)
        where T1 : class where T2 : class
    {
    }
}
