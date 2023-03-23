using Azure.Data.Tables.Poco.Converters;

namespace Azure.Data.Tables.Poco;

public sealed class StoreAsTypeInfoAttribute : StoreAsAttribute
{
    public StoreAsTypeInfoAttribute() : base(new StorablePropertyTypeInfoConverter())
    {
    }
}