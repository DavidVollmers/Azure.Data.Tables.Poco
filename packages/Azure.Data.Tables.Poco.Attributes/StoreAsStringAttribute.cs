using Azure.Data.Tables.Poco.Converters;

namespace Azure.Data.Tables.Poco;

public class StoreAsStringAttribute : StoreAsAttribute
{
    public IFormatProvider? FormatProvider
    {
        get => ((StorablePropertyStringConverter)Converter).FormatProvider;
        set => ((StorablePropertyStringConverter)Converter).FormatProvider = value;
    }

    public StoreAsStringAttribute() : base(new StorablePropertyStringConverter())
    {
    }
}