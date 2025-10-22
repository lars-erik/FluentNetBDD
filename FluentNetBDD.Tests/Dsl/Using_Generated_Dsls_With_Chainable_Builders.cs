using FluentNetBDD.Dsl;
using FluentNetBDD.Generation;
using FluentNetBDD.Tests.Common;
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

public class Using_Generated_Dsls_With_Chainable_Builders 
    : DslTestBase<
        BankCustomerDsl, 
        BankCustomerGivenBuilder, 
        BankCustomerWhenBuilder, 
        BankCustomerThenBuilder
    >
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
            .And.Withdraws(
                amount: 500,
                fromAccount: "123.123.123"
            );

        await Then.Bank
            .HasAccount("123.123.123")
            .WithBalance(1000);

        await State.Verify();
    }

    protected override void AddDrivers(IServiceCollection services)
    {
        services.AddScoped<BankUserGivenDriver>();
        services.AddScoped<IUserWithName>(p => p.GetRequiredService<BankUserGivenDriver>());
        services.AddScoped<IBankUserGivenDriver>(p => p.GetRequiredService<BankUserGivenDriver>());

        services.AddScoped<IBankUserWhenDriver, BankUserWhenDriver>();
        services.AddScoped<IBankDriver, BankDriver>();
    }
}
