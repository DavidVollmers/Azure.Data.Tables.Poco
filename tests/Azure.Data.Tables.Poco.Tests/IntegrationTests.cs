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
            MailAddress = "david@vollmers.org"
        };

        var client = new TableServiceClient("UseDevelopmentStorage=true");

        var tableClient = client.GetTableClient<AccountPoco>();
        Assert.Equal(nameof(AccountPoco), tableClient.Name);

        await tableClient.CreateTableIfNotExistsAsync().ConfigureAwait(false);

        var getPoco = await tableClient.GetIfExistsAsync(accountPoco.Id.ToString(), accountPoco.Id.ToString())
            .ConfigureAwait(false);
        Assert.Null(getPoco);

        var response = await tableClient.AddAsync(accountPoco).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(204, response.Status);

        getPoco = await tableClient.GetIfExistsAsync(accountPoco.Id.ToString(), accountPoco.Id.ToString())
            .ConfigureAwait(false);
        Assert.NotNull(getPoco);
        Assert.Equal(accountPoco.Id, getPoco!.Id);
        Assert.Equal(accountPoco.State, getPoco.State);
        Assert.Null(getPoco.PasswordHash);
        Assert.Equal(accountPoco.MailAddress, getPoco.MailAddress);
        Assert.Equal(accountPoco.CreatedAt, getPoco.CreatedAt);
        Assert.Null(getPoco.LastLoginAt);
        Assert.True(getPoco.UpdatedAt > getPoco.CreatedAt);
        Assert.True(getPoco.IsInternal);

        await tableClient.DeleteTableAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task Test_Query()
    {
        
    }
}