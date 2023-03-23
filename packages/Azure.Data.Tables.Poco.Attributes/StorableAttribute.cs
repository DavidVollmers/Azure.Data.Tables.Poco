namespace Azure.Data.Tables.Poco;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public sealed class StorableAttribute : Attribute
{
    public string? Name { get; }

    public StorableAttribute(string? name = null)
    {
        Name = name;
    }
}