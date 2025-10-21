using FluentNetBDD.Dsl;
using FluentNetBDD.Generation;
using FluentNetBDD.Tests.Dsl.UserFeatures;
using System.Security.Principal;

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

public class BankUserGivenDriver : IBankUserGivenDriver, IUserWithName
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
    private DslState state;
    public BankUserWhenDriver(DslState state)
    {
        this.state = state;
    }

    public async Task Deposits(decimal amount, string toAccount)
    {
        await Task.Delay(10);
        var existing = (decimal?)state.Get(toAccount);
        var newAmount = (existing ?? 0) + amount;
        state.Set(toAccount, newAmount);
    }

    public async Task Withdraws(decimal amount, string fromAccount)
    {
        await Task.Delay(10);
        var existing = (decimal?)state.Get(fromAccount);
        var newAmount = existing ?? 0;
        if (existing.HasValue && existing >= amount)
        {
            newAmount = existing.Value - amount;
        }
        state.Set(fromAccount, newAmount);
    }
}

public class BankDriver : IBankDriver
{
    private DslState state;
    private string contextAccount;

    public BankDriver(DslState state)
    {
        this.state = state;
    }

    public Task HasAccount(string accountNumber)
    {
        Assert.That(state.Get(accountNumber), Is.Not.Null);
        contextAccount = accountNumber;
        return Task.CompletedTask;
    }
    public Task WithBalance(decimal amount)
    {
        Assert.That(state.Get(contextAccount), Is.EqualTo(amount));
        return Task.CompletedTask;
    }
}