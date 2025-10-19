using FluentNetBDD.Dsl.Builders;
using FluentNetBDD.Dsl.Subjunctions;
using FluentNetBDD.Tests.Dsl.UserFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace FluentNetBDD.Tests.Dsl;
using FluentNetBDD.Dsl;

public class Building_Single_Feature_Subjunctions
{
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

        services.AddScoped<DslState>();

        mainProvider = services.BuildServiceProvider();
    }

    [SetUp]
    public void SetUpProvider()
    {
        provider = mainProvider.CreateScope().ServiceProvider;

        untypedDsl = CreateDsl(provider);
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
    public void Constructs_Dynamic_Object_With_Interface_Delegation()
    {
        var dynamicGiven = SubjunctionBuilder.Create<IGivenUserWithName>(provider);

        dynamicGiven.User.WithName("Neo");
        Assert.That(dynamicGiven.User.Name, Is.EqualTo("Neo"));
    }

    [DslTestCase<IGivenUserWithName, IWhen, IThen>]
    public void Constructs_Dsl_With_Specified_Given_Proxy(params Type[] args)
    {
        var dsl = (Dsl<IGivenUserWithName, IWhen, IThen>)untypedDsl;

        dsl.Given.User.WithName("Trinity");
        Assert.That(dsl.Given.User.Name, Is.EqualTo("Trinity"));
    }

    [DslTestCase<IGivenUserWithName, IWhenUserGreeting, IThenUserGreeting>]
    public void Constructs_Dsl_With_Proxies_For_All_Subjunctions(params Type[] args)
    {
        var dsl = (Dsl<IGivenUserWithName, IWhenUserGreeting, IThenUserGreeting>)untypedDsl;

        dsl.Given.User.WithName("Morpheus");
        dsl.When.User.IsGreeted();
        dsl.Then.User.Hears("Hi, Morpheus!");
    }

    private static object CreateDsl(IServiceProvider provider)
    {
        var types = TestContext.CurrentContext.Test.Arguments.OfType<Type[]>().SelectMany(x => x).ToArray();
        if (!types.Any())
        {
            types = TestContext.CurrentContext.Test.Arguments.OfType<Type>().ToArray();
        }

        if (!types.Any())
        {
            types = [typeof(IGiven), typeof(IWhen), typeof(IThen)];
        }

        if (types.Length == 3)
        {
            var genericDslType = DslType.MakeGenericType(types);
            return Activator.CreateInstance(genericDslType, [provider])!;
        }

        throw new Exception("Use params Type[] args with only three types per test to build our Dsl");
    }
}

public class DslTestCaseAttribute<TGiven, TWhen, TThen>()
    : TestCaseAttribute(typeof(TGiven), typeof(TWhen), typeof(TThen));