using FluentNetBDD.Dsl;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Security.Cryptography;

namespace FluentNetBDD;

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

    private IServiceScope scope = null!;
    private IServiceProvider scopedProvider = null!;

    protected abstract void AddDrivers(IServiceCollection services);

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();

        services.AddScoped<DslState>();

        AddDrivers(services);

        var provider = services.BuildServiceProvider();
        scope = provider.CreateScope();
        scopedProvider = scope.ServiceProvider;

        Dsl = Dsl<TGiven, TWhen, TThen>.Create<TDsl>(scopedProvider);
        State = scopedProvider.GetRequiredService<DslState>();
    }

    [TearDown]
    public void TearDown()
    {
        scope.Dispose();
    }

}
