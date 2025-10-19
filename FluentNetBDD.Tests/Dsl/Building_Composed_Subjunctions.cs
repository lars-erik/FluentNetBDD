using FluentNetBDD.Dsl;
using FluentNetBDD.Dsl.Builders;
using FluentNetBDD.Generation;
using FluentNetBDD.Tests.Dsl.UserFeatures;
using Microsoft.Extensions.DependencyInjection;

[assembly:GenerateMarker("HelloWorlder")]
[assembly:GenerateMarker("SillyWorlder", "Hello, silly")]

namespace FluentNetBDD.Tests.Dsl;

public class Building_Composed_Subjunctions
{
    [Test]
    public void Generated_Code_Can_Run()
    {
        var generated = new HelloWorlder();
        Assert.That(generated.ToString(), Is.EqualTo("Hello world!"));
        var generated2 = new SillyWorlder();
        Assert.That(generated2.ToString(), Is.EqualTo("Hello, silly!"));
    }

    private IServiceProvider mainProvider;
    private IServiceProvider provider;
    private object untypedDsl;

    private static readonly Type DslType = typeof(Dsl<,,>);

    [OneTimeSetUp]
    public void SetUpServices()
    {
        var services = new ServiceCollection();

        services.AddScoped<IUserWithName, UserWithName>();
        services.AddScoped<IUserGreetingAction, UserGreetingAction>();
        services.AddScoped<IUserGreetingVerification, UserGreetingVerification>();

        services.AddScoped<IUserWithAgility, UserWithAgilityPocoDriver>();
        services.AddScoped<IUserAgilityActions, UserWithAgilityActionsPocoDriver>();
        services.AddScoped<IUserAgilityVerification, UserWithAgilityVerificationPocoDriver>();

        services.AddScoped<DslState>();

        mainProvider = services.BuildServiceProvider();
    }

    [SetUp]
    public void SetUpProvider()
    {
        provider = mainProvider.CreateScope().ServiceProvider;

        //untypedDsl = ;
    }

    [TearDown]
    public void DisposeProvider()
    {
        (provider as IDisposable)?.Dispose();
    }

    [OneTimeTearDown]
    public void DisposeMainProvider()
    {
        (mainProvider as IDisposable)?.Dispose();
    }

    [Test]
    public void Composes_By_Joining_Interfaces_Per_Conjunction()
    {
        var dsl = new DslBuilder()
            .AddGiven<IUserWithName>()
            .AddGiven<IUserWithAgility>()
            .AddWhen<IUserGreetingAction>()
            .AddWhen<IUserAgilityActions>()
            .AddThen<IUserGreetingVerification>()
            .AddThen<IUserAgilityVerification>()
            .Build(provider);
    }
}

public class DslBuilder
{
    private readonly List<Type> given = new();
    private readonly List<Type> when = new();
    private readonly List<Type> then = new();

    public DslBuilder AddGiven<T>()
    {
        given.Add(typeof(T));
        return this;
    }

    public DslBuilder AddWhen<T>()
    {
        when.Add(typeof(T));
        return this;
    }

    public DslBuilder AddThen<T>()
    {
        then.Add(typeof(T));
        return this;
    }

    public object Build(IServiceProvider provider)
    {
        var createMethod = typeof(SubjunctionBuilder).GetMethod(nameof(SubjunctionBuilder.Create))!;
        var givenType = createMethod.MakeGenericMethod(given.ToArray());
        var whenType = createMethod.MakeGenericMethod(when.ToArray());
        var thenType = createMethod.MakeGenericMethod(then.ToArray());
        throw new NotImplementedException();
    }
}
