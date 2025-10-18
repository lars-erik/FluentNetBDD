namespace FluentNetBDD.Tests.Dsl;
using Dsl = FluentNetBDD.Dsl.Dsl;

public class Subjunctions
{
    protected Dsl Dsl = null!;

    [SetUp]
    public void Setup()
    {
        Dsl = new Dsl();
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