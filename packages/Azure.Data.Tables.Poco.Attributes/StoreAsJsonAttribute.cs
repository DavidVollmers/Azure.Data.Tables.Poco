using Azure.Data.Tables.Poco.Converters;

namespace Azure.Data.Tables.Poco;

public sealed class StoreAsJsonAttribute : StoreAsAttribute
{
    public StoreAsJsonAttribute() : base(new StorablePropertyJsonConverter())
    {
    }
}
