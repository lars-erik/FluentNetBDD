using FluentNetBDD.Dsl;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FluentNetBDD.NUnit;

public abstract class DslTestBase<TDsl, TGiven, TWhen, TThen>
    where TDsl : Dsl<TGiven, TWhen, TThen>
    where TGiven : class
    where TWhen : class
    where TThen : class
{
    protected TDsl Dsl { get; private set; } = null!;
    protected DslState State { get; private set; } = null!;

    protected TGiven Given => Dsl.Given;
    protected TWhen When => Dsl.When;
    protected TThen Then => Dsl.Then;

    protected IServiceProvider Provider => scopedProvider;

    private IServiceProvider mainProvider = null!;
    private IServiceProvider scopedProvider = null!;
    private IServiceScope scope = null!;

    protected abstract void AddDrivers(IServiceCollection services);

    [OneTimeSetUp]
    public void SetUpServices()
    {
        var services = new ServiceCollection();

        services.AddScoped<DslState>();

        AddDrivers(services);

        mainProvider = services.BuildServiceProvider();
    }

    [SetUp]
    public void SetUpScope()
    {
        scope = mainProvider.CreateScope();
        scopedProvider = scope.ServiceProvider;
        Dsl = Dsl<TGiven, TWhen, TThen>.Create<TDsl>(scopedProvider);
        State = scopedProvider.GetRequiredService<DslState>();
    }

    [TearDown]
    public void TearDownScope()
    {
        scope.Dispose();

        if (scopedProvider is IDisposable d)
        {
            d.Dispose();
        }
    }

    [OneTimeTearDown]
    public void DisposeMainProvider()
    {
        if (mainProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

}
