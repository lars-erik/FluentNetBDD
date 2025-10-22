using FluentNetBDD.Dsl.Builders;
using FluentNetBDD.Tests.Dsl.UserFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace FluentNetBDD.Tests.Dsl;
using FluentNetBDD.Dsl;

public class Building_Subjunction_Proxies_With_Drivers
{
    [Test]
    public void Constructs_Dynamic_Object_With_Interface_Delegation()
    {
        var dynamicGiven = DslTermProxyBuilder.Create<IGivenUserWithName>("Given", provider);

        dynamicGiven.User.WithName("Neo");
        Assert.That(dynamicGiven.User.Name, Is.EqualTo("Neo"));
    }

    [Test]
    public void Constructs_Dsl_With_Specified_Given_Proxy()
    {
        var dsl = new Dsl<IGivenUserWithName, IWhen, IThen>(provider);

        dsl.Given.User.WithName("Trinity");
        Assert.That(dsl.Given.User.Name, Is.EqualTo("Trinity"));
    }

    [Test]
    public void Constructs_Dsl_With_Proxies_For_All_Subjunctions()
    {
        var dsl = new Dsl<IGivenUserWithName, IWhenUserGreeting, IThenUserGreeting>(provider);

        dsl.Given.User.WithName("Morpheus");
        dsl.When.User.IsGreeted();
        dsl.Then.User.Hears("Hi, Morpheus!");
    }

    private IServiceProvider mainProvider;
    private IServiceProvider provider;

    [OneTimeSetUp]
    public void SetUpServices()
    {
        var services = new ServiceCollection();

        services.AddScopedDriver<UserWithName>();
        services.AddScopedDriver<UserGreetingAction>();
        services.AddScopedDriver<UserGreetingVerification>();

        services.AddScoped<DslState>();

        mainProvider = services.BuildServiceProvider();
    }

    [SetUp]
    public void SetUpProvider()
    {
        provider = mainProvider.CreateScope().ServiceProvider;
    }

    [TearDown]
    public void DisposeProvider()
    {
        (provider as IDisposable)?.Dispose();
    }

    [OneTimeTearDown]
    public void DisposeMainProvider()
    {
        (mainProvider as IDisposable)?.Dispose();
    }

    public interface IWhen
    {
    }

    public interface IThen
    {
    }
}

