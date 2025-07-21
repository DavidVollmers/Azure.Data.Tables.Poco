using System.Globalization;
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
    public Guid Id { get; init; }

    public AccountState State { get; init; }

    [IgnoreDataMember] public string PasswordHash { get; init; } = null!;

    public string MailAddress { get; init; } = null!;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    [Storable("Timestamp")] public DateTimeOffset UpdatedAt { get; init; }

    public DateTimeOffset? LastLoginAt { get; init; }

    public bool IsInternal => MailAddress.EndsWith("@vollmers.org");

    [StoreAsString] public Uri? AvatarUrl { get; init; }

    [StoreAsString] public Guid? UpdatedBy { get; init; }

    [StoreAsString] public CultureInfo Culture { get; init; } = new("en");

    public decimal Balance { get; init; } = 0.0m;

    public decimal? WithdrawalLimit { get; init; } = null;
}
