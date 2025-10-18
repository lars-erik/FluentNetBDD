# DotNet.BDD

A **fluent, extensible BDD DSL for .NET**  
Inspired by JGiven, Cucumber, and Dave Farley’s four-layer test model.

---

## ✨ Goals

- Enable **fluent, readable Given/When/Then DSLs** that can be extended per feature or domain.  
- Support **dependency injection** for step classes, allowing composition across test layers.  
- Use **Castle.DynamicProxy** (or a similar mechanism) to dynamically attach step interfaces and delegate to registered implementations.
- Allow the **same DSL syntax** to drive different test depths (unit, integration, end-to-end) through configuration.  
- Integrate cleanly with standard test frameworks such as **NUnit**, **xUnit**, or **MSTest**.  
- Support **async/await** where needed, but maintain **natural synchronous readability**.

---

## 🧠 Example Usage

```
private Dsl<GivenForFeature, WhenForFeature, ThenForFeature> Dsl;

[SetUp]
public void SetUp()
{
    var services = new ServiceCollection();
    Dsl.Register(services);
    provider = services.BuildServiceProvider();
}

[Test]
public void Adding_Money()
{
    Given.User.HasAccount();
    When.User.Deposits(100);
    Then.User.HasBalance(100);
}
```

- `Given`, `When`, and `Then` are **proxy-backed objects** implementing domain-specific step interfaces.
- Step interfaces such as `IGivenUser` and `IWhenUser` can be added dynamically via **Castle.DynamicProxy**.
- Actual implementations are resolved from the **service provider** registered via `Dsl.Register(IServiceCollection)`.

---

## 🧩 Architecture Overview

| Layer | Purpose |
|-------|----------|
| **Test DSL Layer** | The fluent façade used in tests (Given/When/Then). |
| **Step Interfaces** | Define readable, domain-level operations (e.g., `IGivenUser`, `IWhenAccount`). |
| **Step Implementations** | Concrete logic, injected via DI; can target API, UI, or domain depending on test depth. |
| **Dynamic Proxy Layer** | Uses `Castle.DynamicProxy` to attach the correct interfaces to the `Given/When/Then` contexts and forward method calls to the registered implementations. |
| **Execution Layer** | Coordinates DI scopes, lifetimes, and asynchronous execution. |

---

## 🧩 Example Step Definitions

```
public interface IGivenUser
{
    void HasAccount();
}

public interface IWhenUser
{
    void Deposits(decimal amount);
}

public interface IThenUser
{
    void HasBalance(decimal expected);
}

public class GivenUserSteps : IGivenUser
{
    private readonly BankContext context;

    public GivenUserSteps(BankContext context) => this.context = context;

    public void HasAccount() => context.Accounts.Add(new Account("TestUser"));
}

public class WhenUserSteps : IWhenUser
{
    private readonly BankContext context;

    public WhenUserSteps(BankContext context) => this.context = context;

    public void Deposits(decimal amount) => context.Deposit("TestUser", amount);
}

public class ThenUserSteps : IThenUser
{
    private readonly BankContext context;

    public ThenUserSteps(BankContext context) => this.context = context;

    public void HasBalance(decimal expected)
        => context.GetBalance("TestUser").Should().Be(expected);
}
```

---

## ⚙️ Planned Features

- [ ] Core `Dsl<TGiven, TWhen, TThen>` class  
- [ ] Automatic registration of step interfaces via DI  
- [ ] `Castle.DynamicProxy` integration to attach interfaces to `Given`, `When`, `Then`  
- [ ] Support for async step methods  
- [ ] Extensible configuration for different test layers (unit/integration/UI)  
- [ ] Optional `Scenario` result reporting  

---

## 🧰 Tech Stack

- .NET 8 / C# 12  
- NUnit (initial integration)  
- Castle.DynamicProxy  
- Microsoft.Extensions.DependencyInjection  
- FluentAssertions  

---

## 📚 References

- [JGiven](https://jgiven.org/)  
- [Ian Cooper – TDD, Where did it all go wrong?](https://www.youtube.com/watch?v=EZ05e7EMOLM)  
- [Acceptance Test Driven Development](https://dojoconsortium.org/assets/ATDD%20-%20How%20to%20Guide.pdf)
- [Cucumber School – Test Layering Concepts](https://school.cucumber.io/)
