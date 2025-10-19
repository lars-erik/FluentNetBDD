using FluentNetBDD.Dsl;
using FluentNetBDD.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentNetBDD.Tests.Dsl.UserFeatures;

public class AgilityFeature
{
    public const string UserAgility = nameof(UserAgility);
    public const string UserJumpedMeters = nameof(UserJumpedMeters);
}

[Actor("User")]
public interface IUserWithAgility
{
    int Agility { get; }
    void WithAgility(int agility);
}

[Actor("User")]
public interface IUserAgilityActions
{
    void Jumps();
    void Runs(int meters);
}

[Actor("User")]
public interface IUserAgilityVerification
{
    void Jumped(int meters);
}

public class UserWithAgilityPocoDriver : IUserWithAgility
{
    public int Agility { get; private set; }

    public void WithAgility(int agility)
    {
        Agility = agility;
    }
}

public class UserWithAgilityActionsPocoDriver : IUserAgilityActions
{
    const double JumpMultiplier = 0.2;

    private readonly DslState state;

    private int runUp = 0;

    public UserWithAgilityActionsPocoDriver(DslState state)
    {
        this.state = state;
    }

    public void Jumps()
    {
        var agility = (int?)state.Get(AgilityFeature.UserAgility) ?? 1;
        state.Set(AgilityFeature.UserJumpedMeters, (int)Math.Ceiling(agility * runUp * JumpMultiplier));
    }
    
    public void Runs(int meters)
    {
        runUp = meters;
    }
}

public class UserWithAgilityVerificationPocoDriver : IUserAgilityVerification
{
    private readonly DslState state;
    
    public UserWithAgilityVerificationPocoDriver(DslState state)
    {
        this.state = state;
    }

    public void Jumped(int meters)
    {
        var jumped = (int?)state.Get(AgilityFeature.UserJumpedMeters);
        Assert.That(jumped, Is.EqualTo(meters));
    }
}