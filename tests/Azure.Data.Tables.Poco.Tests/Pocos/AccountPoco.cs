using System.Runtime.Serialization;

namespace Azure.Data.Tables.Poco.Tests.Pocos;

public enum AccountState
{
    Inactive,
    Active,
    Locked
}

public class AccountPoco
{
    [PartitionKey]
    [RowKey]
    [StoreAsString]
    public Guid Id { get; set; }

    public AccountState State { get; set; }

    [IgnoreDataMember] public string PasswordHash { get; set; } = null!;

    public string MailAddress { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Storable("Timestamp")] public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public bool IsInternal => MailAddress.EndsWith("@vollmers.org");
    
    [StoreAsString] public Uri? AvatarUrl { get; set; }
}
