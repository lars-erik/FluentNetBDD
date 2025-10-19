# FluentNetBDD

**FluentNetBDD** is a type-safe and extensible DSL library for .NET to support ATDD-style (Acceptance-Test Driven Development) and BDD-style (Behavior-Driven Development).  
Building a nice DSL for your tests is not very hard, but to get the best developer experience it may require a lot of plumbing or tight coupling. This project aims to allow composable drivers without the need to write any DSL-specific boilerplate.  
FluentNetBDD combines **Roslyn source generation** and **runtime composition** to provide fluent DSLs that is specialized for each feature — without duplication, losing compile-time safety or IntelliSense. And best of all, you get to work on drivers rather than DSL plumbing.  
The library assumes drivers are resolvable via a `Microsoft.Extensions.DependencyInjection.Abstractions.IServiceProvider`, but there are no other forced dependencies.

---

## ✨ Goals

FluentNetBDD was built to make feature-focused, fluent test DSLs both **easy to extend** and **safe to use**.

- ✅ **Fluent DSL syntax** – natural, Gherkin-style test statements  
  ```csharp
  Given.User.HasAccount();
  When.User.Deposits(100);
  Then.Account.HasBalance(100);
  ```
- ✅ **Static typing & IntelliSense** – every subjunction (`Given`, `When`, `Then`) exposes only the actions relevant to that feature.
- ✅ **Composable actors** – features can mix multiple actor capabilities without resolving to reflection or casting.
- ✅ **Roslyn-powered generation** – compile-time code generation produces feature-specific DSL entrypoints automatically.
- ✅ **Runtime composition** – Castle DynamicProxy builds the actual working objects and delegates to registered actor implementations.

---

## 🧱 Architecture Overview

### 1. DSL Runtime (`FluentNetBDD`)

At runtime, the DSL is built from three main parts:

| Component | Responsibility |
|------------|----------------|
| `Dsl<TGiven, TWhen, TThen>` | Entry point that wires up the Given/When/Then subjunctions. |
| `DslTermProxyBuilder` | Dynamically composes actor interfaces and builds proxy instances for each subjunction. |
| `Interceptors` | Handle property/method delegation across multiple actor interfaces. |

### 2. Source Generation (`FluentNetBDD.Generators`)

Roslyn source generators scan the project for feature-specific actor interfaces and emit a ready-to-use DSL class.  
Example output:

```csharp
// Generated: BankCustomerDsl.g.cs

// The same actor may compose several interfaces
public interface IBankCustomerUserGiven : IBankCustomerGivenUser, INamedUser { ... }
public interface IBankCustomerBankGiven : IGivenBank { ... }
public interface IBankCustomerGiven 
{
    IBankCustomerBankGiven Bank { get; }
    IBankCustomerUserGiven User { get; }
}

// [more actors omitted...]
public interface IBankCustomerWhen { ... }

// [more actors omitted...]
public interface IBankCustomerThen { ... }

public class BankCustomerDsl
    : Dsl<IBankCustomerGiven, IBankCustomerWhen, IBankCustomerThen>
{
    public BankCustomerDsl(IServiceProvider provider) : base(provider) { }
}
```

This allows full IntelliSense and compile-time correctness for each feature without any manual boilerplate.

### 3. Feature Composition

Actor interfaces define domain behavior and should be implemented as registered drivers:

```csharp
[Actor("Bank")]
public interface IGivenBank
{
    void IsOpen();
}

[Actor("User")]
public interface INamedUser
{
    void WithName(string name);
}

[Actor("User")]
public interface IBankCustomerGivenUser
{
    void HasAccount();
}

[Actor("User")]
public interface IBankCustomerWhenUser
{
    void Deposits(int amount);
}

[Actor("Account")]
public interface IThenBankAccount
{
    void HasBalance(int expected);
}
```

The Roslyn generator automatically creates a DSL class combining them.  
Implementations are resolved through dependency injection, so each actor can maintain its own state.

---

## 🧩 Example Test

```csharp
[GenerateDsl(
    "BankCustomer"
    givenTypes: [typeof(IGivenBank), typeof(IBankCustomerGivenUser)],
    whenTypes: [typeof(IBankCustomerWhenUser)],
    thenTypes: [typeof(IThenBankAccount)]
)]

public class Bank_Customer_Transactions
{
    [Test]
    public void Adding_Money_Increases_Balance()
    {
        Given.User.HasAccount();
        Given.Bank.IsOpen();

        When.User.Deposits(100);

        Then.Account.HasBalance(100);
    }

    protected IBankCustomerGiven Given;
    protected IBankCustomerWhen When;
    protected IThenBankAccount Then;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();

        // Implement and register drivers for the composed types
        services.AddTransient<IGivenBank, GivenBankIntegrated>();
        services.AddTransient<IThenBankAccount, ThenBankAccount>();

        services.AddTransient<IBankCustomerGivenUser, BankCustomerGivenUser>();
        services.AddTransient<IBankCustomerWhenUser, BankCustomerWhenUser>();
    
        var provider = services.BuildServiceProvider();

        (Given, When, Then) = new BankCustomerDsl(provider);
    }
}
```

---

## 🔮 Future Work

- **Named actors** for multi-subject tests:  
  ```csharp
  dsl.Given.User.Named("Neo");
  dsl.Then.User.Named("Neo").JumpedLongerThan("Trinity");
  ```
- Library classes to aid in builder-pattern setup before execution of several steps:
  ```csharp
  Given.User
      .WithAccount(balance: 100)
      .And
      .IsLoggedIn();

  await When.User
      .Transfers(50)
      .To("Alice");

  // Then...
  ```
- Enhanced **exception handling and interception** for nested or async operations.  
- DSL documentation and automatic report generation.
- Advanced Roslyn features for **cross-feature composition**.

---

## 🧠 Philosophy

FluentNetBDD aims to make BDD **developer-friendly, discoverable, and statically verifiable** —  
no string-based reflection, no runtime glue, just clean C# with first-class tooling support.

---

## 📚 Inspiration

- [JGiven](https://jgiven.org/)  
- [Ian Cooper – TDD, Where did it all go wrong?](https://www.youtube.com/watch?v=EZ05e7EMOLM)  
- [Acceptance Test Driven Development](https://dojoconsortium.org/assets/ATDD%20-%20How%20to%20Guide.pdf)
- [Cucumber School – Test Layering Concepts](https://school.cucumber.io/)

---

## 📂 Project Layout

```
FluentNetBDD/              # Core DSL runtime
FluentNetBDD.Generation/   # Roslyn support types
FluentNetBDD.Generators/   # Source generators (DslGenerator)
FluentNetBDD.Tests/        # Example & integration tests
```

