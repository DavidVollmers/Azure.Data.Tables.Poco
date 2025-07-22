using Azure.Data.Tables.Poco.Tests.Pocos;
using Azure.Data.Tables.Poco.Transactions;

namespace Azure.Data.Tables.Poco.Tests;

public class TransactionTests
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
        Assert.Equal(exception.FailedTransactionActionIndex, 1);

        var getPoco1 = await tableClient.GetIfExistsAsync(accountPoco1.Id.ToString(), accountPoco1.Id.ToString());

        Assert.NotNull(getPoco1);
    }

    [Fact]
    public async Task Test_MultipleTables()
    {
        var accountPoco = new AccountPoco
        {
            Id = Guid.NewGuid(),
            State = AccountState.Active,
            PasswordHash = Guid.NewGuid().ToString(),
            MailAddress = "david@vollmers.org",
            AvatarUrl = new Uri("https://ui-avatars.com/api/?name=David+Vollmers"),
            Balance = 420.69m,
            WithdrawalLimit = 5000
        };
        var typeInfoPoco = new TypeInfoPoco { Type = typeof(int), Instance = 4 };

        var client = new TableServiceClient("UseDevelopmentStorage=true");

        var tableClient1 = client.GetTableClient<AccountPoco>();
        var tableClient2 = client.GetTableClient<TypeInfoPoco>();

        await tableClient1.CreateTableIfNotExistsAsync();
        await tableClient2.CreateTableIfNotExistsAsync();

        var transaction1 = TypedTableTransaction.For(tableClient1, tableClient2);

        transaction1.Add(accountPoco);

        transaction1.Add(typeInfoPoco);

        await transaction1.SubmitAsync();

        var transaction2 = TypedTableTransaction.For(tableClient1, tableClient2);

        transaction2.Delete(accountPoco);

        transaction2.Add(typeInfoPoco);

        var exception = await Assert.ThrowsAsync<TableTransactionFailedException>(() => transaction2.SubmitAsync());
        Assert.Equal(exception.FailedTransactionActionIndex, 1);

        var getPoco1 = await tableClient1.GetIfExistsAsync(accountPoco.Id.ToString(), accountPoco.Id.ToString());

        Assert.NotNull(getPoco1);
    }
}
