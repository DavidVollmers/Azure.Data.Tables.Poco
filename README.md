## Azure.Data.Tables.Poco

This library provides a simple way to interact with Azure Table Storage using POCOs. It is built on top of the Azure SDK
for Table Storage.

### Getting Started

To get started, you will need to install the `DavidVollmers.Azure.Data.Tables.Poco` package from NuGet:

```bash
dotnet add package DavidVollmers.Azure.Data.Tables.Poco
```

### Usage

To use the library, you will need to create a class that represents the entity you want to store in Table Storage.

```csharp
using DavidVollmers.Azure.Data.Tables.Poco;

// Define the entity class without needing to inherit from TableEntity.
public class AccountPoco
{
    // The Id property is a guid but will be stored as a string in Table Storage.
    // It will be used as the PartitionKey and RowKey.
    [PartitionKey]
    [RowKey]
    [StoreAsString]
    public Guid Id { get; set; }

    // Enums will be stored as integers in Table Storage.
    public AccountState State { get; set; }

    // You can ignore properties that you don't want to store in Table Storage.
    [IgnoreDataMember] public string PasswordHash { get; set; } = null!;

    public string MailAddress { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // You can specify the name of the column in Table Storage.
    // In this case the DateTimeOffset will be mapped to the built-in "Timestamp" property of the entity.
    [Storable("Timestamp")] public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    // You can also store getters-only properties.
    public bool IsInternal => MailAddress.EndsWith("@vollmers.org");
    
    // Complex types are supported by storing them as serializable types. (e.g. JSON, string, etc.)
    [StoreAsString] public Uri? AvatarUrl { get; set; }
}

public enum AccountState
{
    Inactive,
    Active,
    Locked
}
```

Next, you will need to create an instance of `TableClient` and use it to interact with Table Storage.

```csharp
// Create a new instance of your POCO.
var accountPoco = new AccountPoco
{
    Id = Guid.NewGuid(),
    State = AccountState.Active,
    PasswordHash = Guid.NewGuid().ToString(),
    MailAddress = "david@vollmers.org",
    AvatarUrl = new Uri("https://ui-avatars.com/api/?name=David+Vollmers")
};

// Create a new instance of TableServiceClient like normally.
var client = new TableServiceClient("UseDevelopmentStorage=true");

// Use the GetTableClient extension method to get your TypedTableClient instance.
var tableClient = client.GetTableClient<AccountPoco>();

// Create the table if it doesn't exist.
await tableClient.CreateTableIfNotExistsAsync();

// Add the entity to the table.
var response = await tableClient.AddAsync(accountPoco);
