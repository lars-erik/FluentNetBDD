using FluentNetBDD.Dsl;
using FluentNetBDD.Generation;
using FluentNetBDD.Tests.Dsl.BankCustomerFeatures;
using FluentNetBDD.Tests.Dsl.Generated;
using FluentNetBDD.Tests.Dsl.UserFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace FluentNetBDD.Tests.Dsl;

[GenerateDsl(
    "BankCustomer",
    givenTypes: [typeof(IBankUserGivenDriver), typeof(IUserWithName)],
    whenTypes: [typeof(IBankUserWhenDriver)],
    thenTypes: [typeof(IBankDriver)]
)]

[GenerateDsl(
    "BankCustomerBuilder",
    givenTypes: [typeof(IBankUserGivenDriverBuilder)],
    whenTypes: [typeof(IBankUserWhenDriverBuilder)],
    thenTypes: [typeof(IBankDriverBuilder)]
)]

public class Chainable_Drivers
{
    private BankCustomerBuilderDsl dsl;
    protected IBankCustomerBuilderGivenDriver Given;
    protected IBankCustomerBuilderWhenDriver When;
    protected IBankCustomerBuilderThenDriver Then;
    private IServiceProvider scopedProvider;
    private IServiceScope scope;
    private DslState state;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        
        services.AddScoped<BankUserGivenDriver>();
        services.AddScoped<IUserWithName>(p => p.GetRequiredService<BankUserGivenDriver>());
        services.AddScoped<IBankUserGivenDriver>(p => p.GetRequiredService<BankUserGivenDriver>());
        services.AddScoped<IBankCustomerGivenUserDriver>(p => p.GetRequiredService<BankUserGivenDriver>());
        
        services.AddScoped<IBankUserWhenDriver, BankUserWhenDriver>();
        services.AddScoped<IBankDriver, BankDriver>();
        
        services.AddScoped<IBankUserGivenDriverBuilder, BankUserGivenDriverBuilder>();
        services.AddScoped<IBankUserWhenDriverBuilder, BankUserWhenDriverBuilder>();
        services.AddScoped<IBankDriverBuilder, BankDriverBuilder>();
        
        services.AddScoped<DslState>();
        
        // TODO: We're gonna have to BankCustomerBuilderDsl.Register(services) for the builders, or maybe ...

        var provider = services.BuildServiceProvider();
        scope = provider.CreateScope();
        scopedProvider = scope.ServiceProvider;

        dsl = new BankCustomerBuilderDsl(scopedProvider);
        (Given, When, Then) = dsl;

        state = scopedProvider.GetRequiredService<DslState>();
    }

    [TearDown]
    public void TearDown()
    {
        scope.Dispose();
    }

    [Test]
    public async Task Executes_Chained_Async_Calls_In_Order()
    {
        await Given.User
            .WithName("Neo")
            .WithAccount("123.123.123");

        // Let's start easy, then...
        //await When.User
        //    .Deposits(
        //        amount: 1500, 
        //        toAccount: "123.123.123"
        //    )
        //    .And
        //    .Withdraws(
        //        amount: 500,
        //        fromAccount: "123.123.123"
        //    )
        ;

        //await Then.Bank
        //    .HasAccount("123.123.123")
        //    .OwnedBy("Neo")
        //    .WithBalance(1000);

        Assert.That(
            String.Join(Environment.NewLine, state.All.Select(p => $"{p.Key}: {p.Value}")),
            Is.EqualTo
            (
            $"""
            {NamedUserFeature.UserName}: Neo
            {BankCustomerFeature.AccountNumber}: 123.123.123
            """
            )
        );
    }
}
