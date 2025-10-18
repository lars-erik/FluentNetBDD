using FluentNetBDD.Dsl.Subjunctions;
using Microsoft.Extensions.DependencyInjection;

namespace FluentNetBDD.Tests.Dsl;
using FluentNetBDD.Dsl;

public class Subjunctions
{
    protected Dsl<IGiven, IWhen, IThen> Dsl = null!;

    [SetUp]
    public void Setup()
    {
        Dsl = new Dsl<IGiven, IWhen, IThen>(new ServiceCollection().BuildServiceProvider());
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