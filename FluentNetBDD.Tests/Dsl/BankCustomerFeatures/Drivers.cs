using FluentNetBDD.Dsl;
using FluentNetBDD.Generation;
using FluentNetBDD.Tests.Dsl.Generated;
using FluentNetBDD.Tests.Dsl.UserFeatures;

namespace FluentNetBDD.Tests.Dsl.BankCustomerFeatures;

public class BankCustomerFeature
{
    public const string AccountNumber = $"{nameof(BankCustomerFeature)}_{nameof(AccountNumber)}";
}

[Actor("User")]
public interface IBankUserGivenDriver
{
    Task WithAccount(string accountNumber);
}

[Actor("User")]
public interface IBankUserWhenDriver
{
    Task Deposits(decimal amount, string toAccount);
    Task Withdraws(decimal amount, string fromAccount);
}

[Actor("Bank")]
public interface IBankDriver
{
    Task HasAccount(string accountNumber);
    Task WithBalance(decimal amount);
}

public class BankUserGivenDriver : IBankUserGivenDriver, IUserWithName, IBankCustomerGivenUserDriver
{
    private readonly DslState state;
    public string Name { get; private set; } = "";
    public string Account { get; private set; } = "";

    public BankUserGivenDriver(DslState state)
    {
        this.state = state;
    }

    public void WithName(string name)
    {
        Name = name;
        state.Set(NamedUserFeature.UserName, name);
    }

    public async Task WithAccount(string accountNumber)
    {
        await Task.Delay(10);
        Account = accountNumber;
        state.Set(BankCustomerFeature.AccountNumber, accountNumber);
    }
}

public class BankUserWhenDriver : IBankUserWhenDriver
{
    public Task Deposits(decimal amount, string toAccount)
    {
        return Task.CompletedTask;
    }
    public Task Withdraws(decimal amount, string fromAccount)
    {
        return Task.CompletedTask;
    }
}

public class BankDriver : IBankDriver
{
    public Task HasAccount(string accountNumber)
    {
        return Task.CompletedTask;
    }
    public Task WithBalance(decimal amount)
    {
        return Task.CompletedTask;
    }
}