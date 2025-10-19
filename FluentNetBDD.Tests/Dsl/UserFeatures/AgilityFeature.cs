using FluentNetBDD.Dsl;
using FluentNetBDD.Generation;

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
    void Runs(uint meters);
}

[Actor("User")]
public interface IUserAgilityVerification
{
    void Jumped(uint meters);
}

public class UserWithAgility : IUserWithAgility
{
    private readonly DslState state;
    public int Agility { get; private set; }

    public UserWithAgility(DslState state)
    {
        this.state = state;
    }

    public void WithAgility(int agility)
    {
        Agility = agility;
        state.Set(AgilityFeature.UserAgility, agility);
    }
}

public class UserWithAgilityActions : IUserAgilityActions
{
    const double JumpMultiplier = 0.3;

    private readonly DslState state;

    private uint runUp = 0;

    public UserWithAgilityActions(DslState state)
    {
        this.state = state;
    }

    public void Jumps()
    {
        var agility = (int?)state.Get(AgilityFeature.UserAgility) ?? 1;
        var jumpedMeters = (int)Math.Ceiling(agility * Math.Max(1, runUp) * JumpMultiplier);
        state.Set(AgilityFeature.UserJumpedMeters, jumpedMeters);
    }
    
    public void Runs(uint meters)
    {
        runUp = meters;
    }
}

public class UserWithAgilityVerification : IUserAgilityVerification
{
    private readonly DslState state;
    
    public UserWithAgilityVerification(DslState state)
    {
        this.state = state;
    }

    public void Jumped(uint meters)
    {
        var jumped = state.Get(AgilityFeature.UserJumpedMeters);
        Assert.That(jumped, Is.EqualTo(meters));
    }
}