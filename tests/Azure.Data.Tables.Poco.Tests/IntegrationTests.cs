using System.Net.Mail;
using Azure.Data.Tables.Poco.Tests.Pocos;

namespace Azure.Data.Tables.Poco.Tests;

public class IntegrationTests
{
    [Fact]
    public async Task Test_PocoRoundtrip()
    {
        var accountPoco = new AccountPoco
        {
            Id = Guid.NewGuid(),
            State = AccountState.Active,
            PasswordHash = Guid.NewGuid().ToString(),
            MailAddress = "david@vollmers.org",
            AvatarUrl = new Uri("https://ui-avatars.com/api/?name=David+Vollmers")
        };

        var client = new TableServiceClient("UseDevelopmentStorage=true");

        var tableClient = client.GetTableClient<AccountPoco>();
        Assert.Equal(nameof(AccountPoco), tableClient.Name);

        await tableClient.CreateTableIfNotExistsAsync();

        var getPoco = await tableClient.GetIfExistsAsync(accountPoco.Id.ToString(), accountPoco.Id.ToString());
        Assert.Null(getPoco);

        var response = await tableClient.AddAsync(accountPoco);
        Assert.NotNull(response);
        Assert.Equal(204, response.Status);

        getPoco = await tableClient.GetIfExistsAsync(accountPoco.Id.ToString(), accountPoco.Id.ToString());
        Assert.NotNull(getPoco);
        Assert.Equal(accountPoco.Id, getPoco!.Id);
        Assert.Equal(accountPoco.State, getPoco.State);
        Assert.Null(getPoco.PasswordHash);
        Assert.Equal(accountPoco.MailAddress, getPoco.MailAddress);
        Assert.Equal(accountPoco.CreatedAt, getPoco.CreatedAt);
        Assert.Null(getPoco.LastLoginAt);
        Assert.True(getPoco.UpdatedAt > getPoco.CreatedAt);
        Assert.True(getPoco.IsInternal);
        Assert.Equal(accountPoco.AvatarUrl.ToString(), getPoco.AvatarUrl.ToString());

        await tableClient.DeleteTableAsync();
    }

    [Fact]
    public async Task Test_QueryPocos()
    {
        var accountPocos = new List<AccountPoco>();
        for (var i = 0; i <= 10; i++)
        {
            accountPocos.Add(new AccountPoco
            {
                Id = Guid.NewGuid(),
                State = i == 5 ? AccountState.Locked : AccountState.Active,
                MailAddress = $"user{i}@example.com",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-i)
            });
        }

        var client = new TableServiceClient("UseDevelopmentStorage=true");

        var tableClient = client.GetTableClient<AccountPoco>();
        Assert.Equal(nameof(AccountPoco), tableClient.Name);

        // make sure no data exists
        await tableClient.DeleteTableAsync();

        await tableClient.CreateTableIfNotExistsAsync();

        foreach (var accountPoco in accountPocos)
        {
            await tableClient.AddAsync(accountPoco);
        }

        var filter1 = $"{nameof(MailAddress)} ge '{accountPocos[5].MailAddress}'";
        var result1 = await tableClient.QueryAsync(filter1).ToArrayAsync();
        Assert.Collection(result1.OrderBy(a => a.MailAddress),
            result =>
            {
                Assert.NotNull(result);
                Assert.Equal(accountPocos[5].MailAddress, result.MailAddress);
            },
            result =>
            {
                Assert.NotNull(result);
                Assert.Equal(accountPocos[6].MailAddress, result.MailAddress);
            },
            result =>
            {
                Assert.NotNull(result);
                Assert.Equal(accountPocos[7].MailAddress, result.MailAddress);
            },
            result =>
            {
                Assert.NotNull(result);
                Assert.Equal(accountPocos[8].MailAddress, result.MailAddress);
            },
            result =>
            {
                Assert.NotNull(result);
                Assert.Equal(accountPocos[9].MailAddress, result.MailAddress);
            });

        var filter2 = TypedTableClient<AccountPoco>.CreateQueryFilter(a => a.State == AccountState.Locked);
        var result2 = await tableClient.QueryAsync(filter2).ToArrayAsync();
        Assert.Collection(result2,
            result =>
            {
                Assert.NotNull(result);
                Assert.Equal(accountPocos[5].MailAddress, result.MailAddress);
            });

        await tableClient.DeleteTableAsync();
    }

    [Fact]
    public async Task Test_TypeInfoPoco()
    {
        var typeInfoPoco = new TypeInfoPoco
        {
            Instance = Guid.NewGuid().ToString(),
            Type = typeof(string)
        };

        var client = new TableServiceClient("UseDevelopmentStorage=true");

        var tableClient = client.GetTableClient<TypeInfoPoco>();
        Assert.Equal(nameof(TypeInfoPoco), tableClient.Name);

        // make sure no data exists
        await tableClient.DeleteTableAsync();

        await tableClient.CreateTableIfNotExistsAsync();

        var response = await tableClient.AddAsync(typeInfoPoco);
        Assert.NotNull(response);
        Assert.Equal(204, response.Status);

        var filter = $"{nameof(TypeInfoPoco.Type)} eq '{typeof(string).AssemblyQualifiedName}'";
        var results = await tableClient.QueryAsync(filter).ToArrayAsync();
        Assert.Collection(results,
            result =>
            {
                Assert.NotNull(result);
                Assert.Equal(typeInfoPoco.Instance.ToString(), result!.Instance!.ToString());
                Assert.Equal(typeInfoPoco.Type, result.Type);
            });

        await tableClient.DeleteTableAsync();
    }
}