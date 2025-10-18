using FluentNetBDD.Dsl.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace FluentNetBDD.Tests.Dsl;

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

    [Test]
    public void Constructs_Dynamic_Object_With_Interface_Delegation()
    {
        var services = new ServiceCollection();
        services.AddTransient<IUserWithName, UserWithName>();
        var provider = services.BuildServiceProvider();
        var dynamicGiven = SubjunctionBuilder.Create<IGivenUserWithName>(provider);

        dynamicGiven.User.WithName("Neo");
        Assert.That(dynamicGiven.User.Name, Is.EqualTo("Neo"));
    }
}