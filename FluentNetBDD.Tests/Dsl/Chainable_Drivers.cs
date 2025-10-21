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

public class Chainable_Drivers
{
    [Test]
    public async Task Executes_Chained_Async_Calls_In_Order()
    {
        await Given.User
            .WithName("Neo")
            .WithAccount("123.123.123");

        await When.User
            .Deposits(
                amount: 1500,
                toAccount: "123.123.123"
            )
            .And
            .Withdraws(
                amount: 500,
                fromAccount: "123.123.123"
            )
        ;

        await Then.Bank
            .HasAccount("123.123.123")
            //.OwnedBy("Neo")
            .WithBalance(1000);

        Assert.That(
            String.Join(Environment.NewLine, state.All.Select(p => $"{p.Key}: {p.Value}")),
            Is.EqualTo
            (
            $"""
            {NamedUserFeature.UserName}: Neo
            {BankCustomerFeature.AccountNumber}: 123.123.123
            123.123.123: 1000
            """
            )
        );
    }

    private BankCustomerDsl dsl;
    protected BankCustomerGivenBuilder Given;
    protected BankCustomerWhenBuilder When;
    protected BankCustomerThenBuilder Then;
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

        services.AddScoped<IBankUserWhenDriver, BankUserWhenDriver>();
        services.AddScoped<IBankDriver, BankDriver>();

        services.AddScoped<DslState>();

        var provider = services.BuildServiceProvider();
        scope = provider.CreateScope();
        scopedProvider = scope.ServiceProvider;

        dsl = new(scopedProvider);
        (Given, When, Then) = dsl;

        state = scopedProvider.GetRequiredService<DslState>();
    }

    [TearDown]
    public void TearDown()
    {
        scope.Dispose();
    }


}
