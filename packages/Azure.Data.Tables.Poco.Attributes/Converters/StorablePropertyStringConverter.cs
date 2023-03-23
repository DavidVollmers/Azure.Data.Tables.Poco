using System.Reflection;
using Azure.Data.Tables.Poco.Abstractions;

namespace Azure.Data.Tables.Poco.Converters;

internal class StorablePropertyStringConverter : IStorablePropertyConverter
{
    public IFormatProvider? FormatProvider { get; set; }

    public bool IsKeyCompliant => true;

    //TODO support more types
    public bool CanConvert(PropertyInfo propertyInfo)
    {
        return propertyInfo.PropertyType.IsAssignableTo(typeof(IConvertible)) ||
               propertyInfo.PropertyType == typeof(Guid);
    }

    public object? ConvertTo(PropertyInfo propertyInfo, object instance)
    {
        var value = propertyInfo.GetValue(instance);

        return value switch
        {
            null => null,
            IConvertible convertible => convertible.ToString(FormatProvider),
            _ => value.ToString()
        };
    }

    public object? ConvertFrom(PropertyInfo propertyInfo, object? value)
    {
        if (value == null) return null;

        // https://learn.microsoft.com/en-us/dotnet/api/system.iconvertible?view=net-7.0#remarks
        if (value is IConvertible convertible)
        {
            if (propertyInfo.PropertyType == typeof(bool)) return convertible.ToBoolean(FormatProvider);
            if (propertyInfo.PropertyType == typeof(sbyte)) return convertible.ToSByte(FormatProvider);
            if (propertyInfo.PropertyType == typeof(byte)) return convertible.ToByte(FormatProvider);
            if (propertyInfo.PropertyType == typeof(ushort)) return convertible.ToUInt16(FormatProvider);
            if (propertyInfo.PropertyType == typeof(long)) return convertible.ToInt64(FormatProvider);
            if (propertyInfo.PropertyType == typeof(uint)) return convertible.ToUInt32(FormatProvider);
            if (propertyInfo.PropertyType == typeof(ulong)) return convertible.ToUInt64(FormatProvider);
            if (propertyInfo.PropertyType == typeof(float)) return convertible.ToSingle(FormatProvider);
            if (propertyInfo.PropertyType == typeof(int)) return convertible.ToInt32(FormatProvider);
            if (propertyInfo.PropertyType == typeof(short)) return convertible.ToInt16(FormatProvider);
            if (propertyInfo.PropertyType == typeof(decimal)) return convertible.ToDecimal(FormatProvider);
            if (propertyInfo.PropertyType == typeof(DateTime)) return convertible.ToDateTime(FormatProvider);
            if (propertyInfo.PropertyType == typeof(double)) return convertible.ToDouble(FormatProvider);
            if (propertyInfo.PropertyType == typeof(char)) return convertible.ToChar(FormatProvider);
        }

        var stringValue = (string)value;
        
        if (propertyInfo.PropertyType == typeof(Guid)) return Guid.Parse(stringValue);

        return stringValue;
    }
}