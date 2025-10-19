using FluentNetBDD.Dsl;
using FluentNetBDD.Generation;
using FluentNetBDD.Tests.Dsl.UserFeatures;
using Microsoft.Extensions.DependencyInjection;

[assembly:GenerateMarker("HelloWorlder")]
[assembly:GenerateMarker("SillyWorlder", "Hello, silly")]

namespace FluentNetBDD.Tests.Dsl;

[ConsumesDsl(
    "AgileNamedUser",
    [typeof(IUserWithName), typeof(IUserWithAgility)],
    [typeof(IUserGreetingAction), typeof(IUserAgilityActions)],
    [typeof(IUserGreetingVerification), typeof(IUserAgilityVerification)]
)]

public class Building_Generated_Subjunctions
{
    private IServiceProvider mainProvider;
    private IServiceProvider provider;

    private AgileNamedUserDsl dsl;

    [OneTimeSetUp]
    public void SetUpServices()
    {
        var services = new ServiceCollection();

        services.AddScoped<IUserWithName, UserWithName>();
        services.AddScoped<IUserGreetingAction, UserGreetingAction>();
        services.AddScoped<IUserGreetingVerification, UserGreetingVerification>();

        services.AddScoped<IUserWithAgility, UserWithAgility>();
        services.AddScoped<IUserAgilityActions, UserWithAgilityActions>();
        services.AddScoped<IUserAgilityVerification, UserWithAgilityVerification>();

        services.AddScoped<DslState>();

        mainProvider = services.BuildServiceProvider();
    }

    [SetUp]
    public void SetUpProvider()
    {
        provider = mainProvider.CreateScope().ServiceProvider;

        dsl = new AgileNamedUserDsl(provider);
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

    [Test]
    public void Composes_By_Joining_Interfaces_Per_Conjunction()
    {
        dsl.Given.User.WithName("Neo");
        dsl.Given.User.WithAgility(10);

        dsl.When.User.Jumps();

        dsl.Then.User.Jumped(5);
    }

    [Test]
    public void Generated_Code_Can_Run()
    {
        var generated = new HelloWorlder();
        Assert.That(generated.ToString(), Is.EqualTo("Hello world!"));
        var generated2 = new SillyWorlder();
        Assert.That(generated2.ToString(), Is.EqualTo("Hello, silly!"));
    }
}
