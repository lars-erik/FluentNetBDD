using FluentNetBDD.Dsl;
using FluentNetBDD.Generation;
using FluentNetBDD.Tests.Dsl.Generated;
using FluentNetBDD.Tests.Dsl.UserFeatures;
using Microsoft.Extensions.DependencyInjection;
using Throws = NUnit.Framework.Throws;

namespace FluentNetBDD.Tests.Dsl;

[GenerateDsl(
    "AgileNamedUser",
    givenTypes: [typeof(IUserWithName), typeof(IUserWithAgility)],
    whenTypes: [typeof(IUserGreetingAction), typeof(IUserAgilityActions)],
    thenTypes: [typeof(IUserGreetingVerification), typeof(IUserAgilityVerification)]
)]

public class Using_Generated_Dsls
{
    protected IAgileNamedUserGivenDriver Given;
    protected IAgileNamedUserWhenDriver When;
    protected IAgileNamedUserThenDriver Then;

    [Test]
    public void Has_Single_Instance_Scoped_Drivers()
    {
        var stateA = provider.GetRequiredService<DslState>();
        var stateB = provider.GetRequiredService<DslState>();
        Assert.That(stateA, Is.SameAs(stateB));

        var userWithNameA = provider.GetRequiredService<IUserWithName>();
        var userWithNameB = provider.GetRequiredService<IUserWithName>();
        Assert.That(userWithNameA, Is.SameAs(userWithNameB).And.InstanceOf<UserWithName>());

        userWithNameA.WithName("THE user");
        Assert.That(userWithNameB.Name, Is.EqualTo("THE user").And.EqualTo(stateA.Get(NamedUserFeature.UserName)));
    }

    [Test]
    public void Resolves_Scoped_Implementations_From_Provider()
    {
        var state = provider.GetRequiredService<DslState>();
        var providedUserWithName = provider.GetRequiredService<IUserWithName>();
        
        Given.User.WithName("Neo");

        Assert.That(state.Get(NamedUserFeature.UserName), Is.EqualTo("Neo"));
        Assert.That(providedUserWithName.Name, Is.EqualTo("Neo"));
    }

    [Test]
    public void Composes_By_Joining_Interfaces_Per_Conjunction()
    {
        const int expectedJumpMeters = 3;
        const int expectedJumpMetersWithRunUp = 15;

        Given.User.WithName("Neo");
        Given.User.WithAgility(10);

        When.User.Jumps();
        Then.User.Jumped(expectedJumpMeters);

        When.User.Runs(5);
        When.User.Jumps();
        Then.User.Jumped(expectedJumpMetersWithRunUp);

        var state = provider.GetRequiredService<DslState>();
        Assert.That(state.Get(AgilityFeature.UserJumpedMeters), Is.EqualTo(expectedJumpMetersWithRunUp));
    }

    [Test]
    public void Then_Implementation_Asserts_Invalid_Values()
    {
        When.User.Jumps();

        Assert.That(
            () => Then.User.Jumped(100),
            Throws
                .TargetInvocationException
                .With.Property(nameof(Exception.InnerException))
                .InstanceOf<AssertionException>()
        );
    }

    private IServiceProvider mainProvider;
    private IServiceProvider provider;

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

        (Given, When, Then) = new Dsl<IAgileNamedUserGivenDriver, IAgileNamedUserWhenDriver, IAgileNamedUserThenDriver>(provider);
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
}
