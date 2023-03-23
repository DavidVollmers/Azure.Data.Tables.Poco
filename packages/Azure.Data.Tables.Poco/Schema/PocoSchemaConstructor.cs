using System.Reflection;

namespace Azure.Data.Tables.Poco.Schema;

internal class PocoSchemaConstructor
{
    private readonly ConstructorInfo _constructorInfo;
    private readonly ParameterInfo[] _parameterInfo;

    private PocoSchemaConstructor(ConstructorInfo constructorInfo, ParameterInfo[] parameterInfo)
    {
        _constructorInfo = constructorInfo;
        _parameterInfo = parameterInfo;
    }

    public object CreateInstance(IDictionary<string, object> properties)
    {
        var parameters = new List<object?>();
        foreach (var parameterInfo in _parameterInfo)
        {
            if (!properties.ContainsKey(parameterInfo.Name!))
            {
                var defaultValue = parameterInfo.ParameterType.IsValueType
                    ? Activator.CreateInstance(parameterInfo.ParameterType)
                    : null;
                parameters.Add(defaultValue);
            }
            else
            {
                parameters.Add(properties[parameterInfo.Name!]);
            }
        }

        return _constructorInfo.Invoke(parameters.ToArray());
    }

    public static PocoSchemaConstructor CreateFromType(Type type, IList<PocoSchemaProperty> getProperties,
        IList<PocoSchemaProperty> setProperties)
    {
        var constructors = type.GetConstructors().Select(i => new { i, p = i.GetParameters() }).ToArray();

        var pocoConstructors = constructors.Where(c => c.i.GetCustomAttribute<PocoConstructorAttribute>(true) != null)
            .ToArray();
        if (pocoConstructors.Length > 1)
            throw new InvalidOperationException($"More than one POCO constructor specified on type '{type.FullName}'.");

        var constructor = pocoConstructors.FirstOrDefault() ?? constructors.FirstOrDefault(c => c.p.Length == 0);
        if (constructor == null)
            throw new InvalidOperationException(
                $"No valid POCO constructor found on type '{type.FullName}'. Please use the PocoConstructorAttribute or define a parameterless constructor.");

        if (constructor.p.Length > 0)
        {
            foreach (var parameterInfo in constructor.p)
            {
                var propertyInfo = getProperties.FirstOrDefault(p =>
                    string.Equals(p.Name, parameterInfo.Name, StringComparison.CurrentCultureIgnoreCase));

                if (propertyInfo == null)
                {
                    var constructorSignature =
                        $"Void .ctor({string.Join(", ", constructor.p.Select(pi => pi.ParameterType.FullName))})";
                    throw new InvalidOperationException(
                        $"Each parameter in POCO constructor '{constructorSignature}' on type '{type.FullName}' must bind to a storable property. Each parameter name must match with a storable property on the object. The match can be case-insensitive.");
                }

                if (propertyInfo.CanWrite) setProperties.Remove(propertyInfo);
            }
        }

        return new PocoSchemaConstructor(constructor.i, constructor.p.OrderBy(cp => cp.Position).ToArray());
    }
}