using Azure.Data.Tables.Poco.Schema;
using Azure.Data.Tables.Poco.Tests.Pocos;
using NSubstitute;

namespace Azure.Data.Tables.Poco.Tests;

public class SubstituteTests
{
    [Fact]
    public async Task Test_SubstituteTypedTableClient()
    {
        var entities = new List<AccountPoco>();
        var schema = PocoSchema.CreateFromType(typeof(AccountPoco));

        var typedTableClient = Substitute.For<ITypedTableClient<AccountPoco>>();

        typedTableClient
            .When(t => t.AddAsync(Arg.Any<AccountPoco>()))
            .Do(call =>
            {
                var entity = (AccountPoco)call[0];

                entities.Add(entity);
            });

        typedTableClient
            .When(t => t.DeleteAsync(Arg.Any<AccountPoco>()))
            .Do(call =>
            {
                var entity = (AccountPoco)call[0];

                entities.Remove(entity);
            });

        typedTableClient
            .GetIfExistsAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call =>
            {
                var partitionKey = (string)call[0];
                var rowKey = (string)call[1];

                return (from entity in entities
                    let pk = (string)schema.PartitionKey.GetValue(entity)!
                    let rk = (string)schema.RowKey.GetValue(entity)!
                    where pk == partitionKey && rk == rowKey
                    select entity).FirstOrDefault();
            });

        var accountPoco = new AccountPoco
        {
            Id = Guid.NewGuid(),
            State = AccountState.Active,
            PasswordHash = Guid.NewGuid().ToString(),
            MailAddress = "david@vollmers.org",
            AvatarUrl = new Uri("https://ui-avatars.com/api/?name=David+Vollmers")
        };

        await typedTableClient.AddAsync(accountPoco);

        var getPoco = await typedTableClient.GetIfExistsAsync(accountPoco.Id.ToString(), accountPoco.Id.ToString());

        Assert.NotNull(getPoco);
        Assert.Equal(accountPoco.Id, getPoco.Id);

        await typedTableClient.DeleteAsync(accountPoco);

        getPoco = await typedTableClient.GetIfExistsAsync(accountPoco.Id.ToString(), accountPoco.Id.ToString());

        Assert.Null(getPoco);
    }
}
