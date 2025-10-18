using FluentNetBDD.Dsl.Builders;
using FluentNetBDD.Dsl.Subjunctions;
using Microsoft.Extensions.DependencyInjection;

namespace FluentNetBDD.Tests.Dsl;
using FluentNetBDD.Dsl;

public class Building_Single_Feature_Subjunctions
{
    public class UserGreetingFeature
    {
        public const string UserName = nameof(UserName);
        public const string UserGreeting = nameof(UserGreeting);
    }

    public interface IUserWithName
    {
        string Name { get; }

        // TODO: Sooner or later we want to be able to chain everything...
        void WithName(string name);
    }

    public interface IUserGreetingAction
    {
        void IsGreeted();
    }

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