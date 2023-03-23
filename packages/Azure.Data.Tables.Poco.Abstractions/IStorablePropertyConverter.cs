using System.Reflection;

namespace Azure.Data.Tables.Poco.Abstractions;

public interface IStorablePropertyConverter
{
    bool IsKeyCompliant { get; }
    
    bool CanConvert(PropertyInfo propertyInfo);

    object? ConvertTo(PropertyInfo propertyInfo, object instance);

    object? ConvertFrom(PropertyInfo propertyInfo, object? value);
}