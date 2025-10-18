using FluentNetBDD.Dsl.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace FluentNetBDD.Tests.Dsl;
using FluentNetBDD.Dsl;

public class Building_Feature_Subjunctions
{
    public interface IUserWithName
    {
        string Name { get; }

        // TODO: Sooner or later we want to be able to chain everything...
        void WithName(string name);
    }

    public interface IGivenUserWithName
    {
        IUserWithName User { get; }
    }

    class UserWithName : IUserWithName
    {
        public string Name { get; private set; } = string.Empty;
        public void WithName(string name)
        {
            Name = name;
        }
    }

    private IServiceProvider provider;

    [SetUp]
    public void SetUpServices()
    {
        var services = new ServiceCollection();
        services.AddTransient<IUserWithName, UserWithName>();
        provider = services.BuildServiceProvider();
    }

    [TearDown]
    public void DisposeServices()
    {
        (provider as IDisposable)?.Dispose();
    }

    [Test]
    public void Constructs_Dynamic_Object_With_Interface_Delegation()
    {
        var dynamicGiven = SubjunctionBuilder.Create<IGivenUserWithName>(provider);

        dynamicGiven.User.WithName("Neo");
        Assert.That(dynamicGiven.User.Name, Is.EqualTo("Neo"));
    }

    [Test]
    public void Constructs_Dsl_With_Specified_Given_Proxy()
    {
        var dsl = new Dsl<IGivenUserWithName>(provider);
        dsl.Given.User.WithName("Trinity");
        Assert.That(dsl.Given.User.Name, Is.EqualTo("Trinity"));
    }
}