using FluentNetBDD.Dsl;
using FluentNetBDD.Generation;

namespace FluentNetBDD.Tests.Dsl.UserFeatures;

public class NamedUserFeature
{
    public const string UserName = nameof(UserName);
    public const string UserGreeting = nameof(UserGreeting);
}

[Actor("User")]
public interface IUserWithName
{
    string Name { get; }

    // TODO: Sooner or later we want to be able to chain everything...
    void WithName(string name);
}

[Actor("User")]
public interface IUserGreetingAction
{
    void IsGreeted();
}

[Actor("User")]
public interface IUserGreetingVerification
{
    void Hears(string greeting);
}

/// <summary>
/// This is here for the manual example in <see cref="Building_Single_Feature_Subjunctions"/>.
/// It should rather be generated via the <see cref="GenerateDslAttribute"/> attribute.
/// </summary>
public interface IGivenUserWithName
{
    IUserWithName User { get; }
}

/// <summary>
/// This is here for the manual example in <see cref="Building_Single_Feature_Subjunctions"/>.
/// It should rather be generated via the <see cref="GenerateDslAttribute"/> attribute.
/// </summary>
public interface IWhenUserGreeting
{
    IUserGreetingAction User { get; }
}

/// <summary>
/// This is here for the manual example in <see cref="Building_Single_Feature_Subjunctions"/>.
/// It should rather be generated via the <see cref="GenerateDslAttribute"/> attribute.
/// </summary>
public interface IThenUserGreeting
{
    IUserGreetingVerification User { get; }
}

class UserWithName : IUserWithName
{
    private readonly DslState state;
    public string Name { get; private set; } = string.Empty;
    public void WithName(string name)
    {
        Name = name;
        state.Set(NamedUserFeature.UserName, name);
    }

    public UserWithName(DslState state)
    {
        this.state = state;
    }
}

class UserGreetingAction : IUserGreetingAction
{
    private readonly DslState state;

    public UserGreetingAction(DslState state)
    {
        this.state = state;
    }

    public void IsGreeted()
    {
        var userName = state.Get(NamedUserFeature.UserName);
        state.Set(NamedUserFeature.UserGreeting, $"Hi, {userName}!");
    }
}

class UserGreetingVerification : IUserGreetingVerification
{
    private readonly DslState state;
    public UserGreetingVerification(DslState state)
    {
        this.state = state;
    }
    public void Hears(string greeting)
    {
        var actualGreeting = state.Get(NamedUserFeature.UserGreeting);
        Assert.That(actualGreeting, Is.EqualTo(state.Get(NamedUserFeature.UserGreeting)));
    }
}