using System.Reflection;
using Azure.Data.Tables.Poco.Abstractions;

namespace Azure.Data.Tables.Poco.Converters;

internal class StorablePropertyTypeInfoConverter : IStorablePropertyConverter
{
    public bool IsKeyCompliant => true;

    public bool CanConvert(PropertyInfo propertyInfo)
    {
        return propertyInfo.PropertyType == typeof(Type);
    }

    public object? ConvertTo(PropertyInfo propertyInfo, object instance)
    {
        var value = propertyInfo.GetValue(instance);

        // ReSharper disable once MergeConditionalExpression
        return value == null ? null : ((Type)value).AssemblyQualifiedName;
    }

    public object? ConvertFrom(PropertyInfo propertyInfo, object? value)
    {
        return value == null ? null : Type.GetType((string)value);
    }
}