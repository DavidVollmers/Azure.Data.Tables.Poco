using Azure.Data.Tables.Poco.Converters;

namespace Azure.Data.Tables.Poco;

public class StoreAsStringAttribute : StoreAsAttribute
{
    public StoreAsStringAttribute() : base(new StorablePropertyStringConverter())
    {
    }
}
