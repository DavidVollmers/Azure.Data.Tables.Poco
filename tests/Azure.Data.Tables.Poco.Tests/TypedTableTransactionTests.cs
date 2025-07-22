using Azure.Data.Tables.Poco.Tests.Pocos;

namespace Azure.Data.Tables.Poco.Tests;

public class TypedTableTransactionTests
{
    [Fact]
    public async Task Test_Transaction()
    {
        var accountPoco1 = new AccountPoco
        {
            Id = Guid.NewGuid(),
            State = AccountState.Active,
            PasswordHash = Guid.NewGuid().ToString(),
            MailAddress = "david@vollmers.org",
            AvatarUrl = new Uri("https://ui-avatars.com/api/?name=David+Vollmers"),
            Balance = 420.69m,
            WithdrawalLimit = 5000
        };
        var accountPoco2 = new AccountPoco
        {
            Id = Guid.NewGuid(),
            State = AccountState.Active,
            PasswordHash = Guid.NewGuid().ToString(),
            MailAddress = "david@vollmers.org",
            AvatarUrl = new Uri("https://ui-avatars.com/api/?name=David+Vollmers"),
        };

        var client = new TableServiceClient("UseDevelopmentStorage=true");

        var tableClient = client.GetTableClient<AccountPoco>();

        await tableClient.CreateTableIfNotExistsAsync();

        var transaction1 = tableClient.CreateTransaction();

        transaction1.Add(accountPoco1);

        transaction1.Add(accountPoco2);

        await transaction1.SubmitAsync();

        var transaction2 = tableClient.CreateTransaction();

        transaction2.Delete(accountPoco1);

        transaction2.Add(accountPoco2);

        var exception = await Assert.ThrowsAsync<TableTransactionFailedException>(() => transaction2.SubmitAsync());
        Assert.Equal(1, exception.FailedTransactionActionIndex);
        
        var getPoco1 = await tableClient.GetIfExistsAsync(accountPoco1.Id.ToString(), accountPoco1.Id.ToString());
        
        Assert.NotNull(getPoco1);
    }
}
