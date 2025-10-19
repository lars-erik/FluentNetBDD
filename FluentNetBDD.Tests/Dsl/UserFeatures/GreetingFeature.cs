using FluentNetBDD.Dsl;
using FluentNetBDD.Generation;

namespace FluentNetBDD.Tests.Dsl.UserFeatures;

public class UserGreetingFeature
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

public interface IGivenUserWithName
{
    IUserWithName User { get; }
}

public interface IWhenUserGreeting
{
    IUserGreetingAction User { get; }
}

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
        state.Set(UserGreetingFeature.UserName, name);
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
        var userName = state.Get(UserGreetingFeature.UserName);
        state.Set(UserGreetingFeature.UserGreeting, $"Hi, {userName}!");
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
        var actualGreeting = state.Get(UserGreetingFeature.UserGreeting);
        Assert.That(actualGreeting, Is.EqualTo(state.Get(UserGreetingFeature.UserGreeting)));
    }
}