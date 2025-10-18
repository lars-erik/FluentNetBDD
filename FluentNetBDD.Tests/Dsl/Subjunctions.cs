using Microsoft.Extensions.DependencyInjection;

namespace FluentNetBDD.Tests.Dsl;
using FluentNetBDD.Dsl;

public class Subjunctions
{
    public interface IGiven {}

    protected Dsl<IGiven> Dsl = null!;

    [SetUp]
    public void Setup()
    {
        Dsl = new Dsl<IGiven>(new ServiceCollection().BuildServiceProvider());
    }

    [Test]
    public void Dsl_Has_Given()
    {
        Assert.That(Dsl.Given, Is.Not.Null);
    }

    [Test]
    public void Dsl_Has_When()
    {
        Assert.That(Dsl.When, Is.Not.Null);
    }

    [Test]
    public void Dsl_Has_Then()
    {
        Assert.That(Dsl.Then, Is.Not.Null);
    }
}