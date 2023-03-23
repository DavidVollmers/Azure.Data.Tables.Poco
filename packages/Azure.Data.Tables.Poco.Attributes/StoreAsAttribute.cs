using Azure.Data.Tables.Poco.Abstractions;

namespace Azure.Data.Tables.Poco;

[AttributeUsage(AttributeTargets.Property)]
public class StoreAsAttribute : Attribute
{
    public IStorablePropertyConverter Converter { get; }

    public StoreAsAttribute(IStorablePropertyConverter converter)
    {
        Converter = converter;
    }
}