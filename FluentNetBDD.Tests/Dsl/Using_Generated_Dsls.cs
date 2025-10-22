using FluentNetBDD.Dsl;
using FluentNetBDD.Generation;
using FluentNetBDD.NUnit;
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

/*
 * This class tests the "raw" DSL drivers.
 * We presume everyone want to use the chainable builders, so there is no generated non-generic class.
 * Hence the extra long list of generic parameters.
 */
public class Using_Generated_Dsls : DslTestBase<
    Dsl<IAgileNamedUserGivenDriver, IAgileNamedUserWhenDriver, IAgileNamedUserThenDriver>,
    IAgileNamedUserGivenDriver, IAgileNamedUserWhenDriver, IAgileNamedUserThenDriver>
{
    [Test]
    public void Has_Single_Instance_Scoped_Drivers()
    {
        var stateA = Provider.GetRequiredService<DslState>();
        var stateB = Provider.GetRequiredService<DslState>();
        Assert.That(stateA, Is.SameAs(stateB));

        var userWithNameA = Provider.GetRequiredService<IUserWithName>();
        var userWithNameB = Provider.GetRequiredService<IUserWithName>();
        Assert.That(userWithNameA, Is.SameAs(userWithNameB).And.InstanceOf<UserWithName>());

        userWithNameA.WithName("THE user");
        Assert.That(userWithNameB.Name, Is.EqualTo("THE user").And.EqualTo(stateA.Get(NamedUserFeature.UserName)));
    }

    [Test]
    public void Resolves_Scoped_Implementations_From_Provider()
    {
        var state = Provider.GetRequiredService<DslState>();
        var providedUserWithName = Provider.GetRequiredService<IUserWithName>();
        
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

        Assert.That(State.Get(AgilityFeature.UserJumpedMeters), Is.EqualTo(expectedJumpMetersWithRunUp));
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

    protected override void AddDrivers(IServiceCollection services)
    {
        services.AddScopedDriver<UserWithName>();
        services.AddScopedDriver<UserGreetingAction>();
        services.AddScopedDriver<UserGreetingVerification>();

        services.AddScopedDriver<UserWithAgility>();
        services.AddScopedDriver<UserWithAgilityActions>();
        services.AddScopedDriver<UserWithAgilityVerification>();
    }
}
