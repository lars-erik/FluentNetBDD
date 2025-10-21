using FluentNetBDD.Generation;
using System.Runtime.CompilerServices;
using FluentNetBDD.Tests.Dsl.Generated;
using FluentNetBDD.Tests.Dsl.UserFeatures;

namespace FluentNetBDD.Tests.Dsl.BankCustomerFeatures;

/*
 * HOPEFULLY WE CAN GENERATE THIS FILE
 */

[Actor("User")]
public interface IBankUserGivenDriverBuilder
{
    IBankUserGivenDriverBuilder WithName(string name);
    IBankUserGivenDriverBuilder WithAccount(string accountNumber);

    // We can likely just have this here or even higher in the hierarchy
    // public TaskAwaiter GetAwaiter() => Task.CompletedTask.GetAwaiter();
    ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext);
    TaskAwaiter GetAwaiter();
}

[Actor("User")]
public interface IBankUserWhenDriverBuilder
{
    IBankUserWhenDriverBuilder Deposits(decimal amount, string toAccount);
    IBankUserWhenDriverBuilder Withdraws(decimal amount, string fromAccount);
}

[Actor("Bank")]
public interface IBankDriverBuilder
{
    IBankDriverBuilder HasAccount(string accountNumber);
    IBankDriverBuilder WithBalance(decimal amount);
}

public class BankUserGivenDriverBuilder : IBankUserGivenDriverBuilder
{
    private readonly IBankCustomerGivenUserDriver driver;

    public BankUserGivenDriverBuilder(IBankCustomerGivenUserDriver driver)
    {
        this.driver = driver;
    }

    public IBankUserGivenDriverBuilder WithName(string name)
    {
        actions.Add(async () => await Task.Run(() => driver.WithName(name)));
        return this;
    }

    public IBankUserGivenDriverBuilder WithAccount(string accountNumber)
    {
        actions.Add(async () => await driver.WithAccount(accountNumber));
        return this;
    }

    // TODO: Do we want something else than list, like Queue?
    private Task? task = null;
    private readonly List<Func<Task>> actions = new();

    protected Task ToTask()
    {
        Func<Func<Task>, Task> selector = async action =>
        {
            await action();
        };
        if (task == null)
        {
            var tasks = actions.Select(selector);
            task = Task.WhenAll(tasks);
        }
        return task;
    }

    public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext) => ToTask().ConfigureAwait(continueOnCapturedContext);
    
    public TaskAwaiter GetAwaiter() => ToTask().GetAwaiter();

    public static implicit operator Task(BankUserGivenDriverBuilder builder) => builder.ToTask();
}

public class BankUserWhenDriverBuilder : IBankUserWhenDriverBuilder
{
    public IBankUserWhenDriverBuilder Deposits(decimal amount, string toAccount)
    {
        return this;
    }

    public IBankUserWhenDriverBuilder Withdraws(decimal amount, string fromAccount)
    {
        return this;
    }
}

public class BankDriverBuilder : IBankDriverBuilder
{
    public IBankDriverBuilder HasAccount(string accountNumber)
    {
        return this;
    }

    public IBankDriverBuilder WithBalance(decimal amount)
    {
        return this;
    }
}
