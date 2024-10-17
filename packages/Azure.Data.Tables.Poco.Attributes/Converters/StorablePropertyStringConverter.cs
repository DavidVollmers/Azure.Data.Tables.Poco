using System.Globalization;
using System.Reflection;
using Azure.Data.Tables.Poco.Abstractions;

namespace Azure.Data.Tables.Poco.Converters;

internal class StorablePropertyStringConverter : IStorablePropertyConverter
{
    private static readonly IFormatProvider FormatProvider = CultureInfo.InvariantCulture;

    public bool IsKeyCompliant => true;

    //TODO support more types
    public bool CanConvert(PropertyInfo propertyInfo)
    {
        return propertyInfo.PropertyType.IsAssignableTo(typeof(IConvertible)) ||
               propertyInfo.PropertyType == typeof(Guid?) ||
               propertyInfo.PropertyType == typeof(Guid) ||
               propertyInfo.PropertyType == typeof(Uri);
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
        switch (value)
        {
            case null:
                return null;
            // https://learn.microsoft.com/en-us/dotnet/api/system.iconvertible?view=net-7.0#remarks
            case IConvertible convertible when propertyInfo.PropertyType == typeof(bool):
                return convertible.ToBoolean(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(sbyte):
                return convertible.ToSByte(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(byte):
                return convertible.ToByte(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(ushort):
                return convertible.ToUInt16(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(long):
                return convertible.ToInt64(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(uint):
                return convertible.ToUInt32(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(ulong):
                return convertible.ToUInt64(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(float):
                return convertible.ToSingle(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(int):
                return convertible.ToInt32(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(short):
                return convertible.ToInt16(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(decimal):
                return convertible.ToDecimal(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(DateTime):
                return convertible.ToDateTime(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(double):
                return convertible.ToDouble(FormatProvider);
            case IConvertible convertible when propertyInfo.PropertyType == typeof(char):
                return convertible.ToChar(FormatProvider);
        }

        var stringValue = (string)value;

        if (propertyInfo.PropertyType == typeof(Guid?) || propertyInfo.PropertyType == typeof(Guid))
            return Guid.Parse(stringValue);
        if (propertyInfo.PropertyType == typeof(Uri)) return new Uri(stringValue, UriKind.RelativeOrAbsolute);

        return stringValue;
    }
}
