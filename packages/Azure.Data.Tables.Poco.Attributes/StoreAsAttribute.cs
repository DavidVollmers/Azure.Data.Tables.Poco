using Azure.Data.Tables.Poco.Abstractions;

namespace Azure.Data.Tables.Poco;

[AttributeUsage(AttributeTargets.Property)]
public abstract class StoreAsAttribute : Attribute
{
    public IStorablePropertyConverter Converter { get; }

    protected StoreAsAttribute(IStorablePropertyConverter converter)
    {
        Converter = converter ?? throw new ArgumentNullException(nameof(converter));
    }
}
