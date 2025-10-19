# Copilot Instructions for FluentNetBDD

FluentNetBDD is a **type-safe, extensible BDD (Behavior-Driven Development) DSL framework** for .NET.  
It combines **Roslyn source generation** and **runtime composition using Castle.DynamicProxy** to provide fluent, per-feature test DSLs with full IntelliSense and compile-time safety.

---

## 🎯 Purpose

Copilot should help users extend or consume FluentNetBDD by suggesting **idiomatic driver code**, **actor interfaces**, and **test DSL usage**, *not* generic BDD examples or Gherkin text.

Generated suggestions should:
- Prefer **C# 12+** syntax and `.NET 9` conventions.
- Stay **framework-agnostic** (no hard dependency on xUnit, NUnit, or SpecFlow).
- Encourage **strong typing** and **dependency-injection-driven design**.
- Avoid code that introduces reflection, runtime casting, or string-based step definitions.

---

## 🧠 Core Concepts

FluentNetBDD uses **subjunctions** and **actors** to compose domain-specific DSLs.

| Concept | Description |
|----------|--------------|
| `Given`, `When`, `Then` | Subjunctions that form the top-level DSL entrypoints. |
| **Actor** | A domain participant exposing behavior through interfaces like `IGivenUser`, `IWhenUser`, `IThenUser`. |
| **Driver** | The concrete implementation of an actor, registered in the DI container. |
| **Dsl\<TGiven, TWhen, TThen\>** | A typed DSL entrypoint that builds subjunction proxies via `SubjunctionBuilder`. |
| **Source Generator** | Emits strongly typed DSLs automatically for features (e.g. `BankCustomerDsl`). |
| **Interceptor** | Handles property/method calls across composed actor interfaces. |

---

## 🧩 Example Pattern

When Copilot generates examples, it should **follow this structure**:

```csharp
[GenerateDsl(
    "BankCustomer",
    givenTypes: [typeof(IGivenBank), typeof(IBankCustomerGivenUser)],
    whenTypes: [typeof(IBankCustomerWhenUser)],
    thenTypes: [typeof(IThenBankAccount)]
)]
public class Bank_Customer_Transactions
{
    [Test]
    public async Task Adding_Money_Increases_Balance()
    {
        await Given.User.WithName("Alice").HasAccount();
        await When.User.Transfers(50).To.User("Bob");
        await Then.Account.HasBalance(50);
    }

    protected IBankCustomerGiven Given;
    protected IBankCustomerWhen When;
    protected IBankCustomerThen Then;
}
```

---

## ⚙️ Guidelines for Copilot

When suggesting code inside this repository:

### ✅ Prefer
- Defining new **actor interfaces** with clear prefixes (`IGiven`, `IWhen`, `IThen`).
- Creating **driver classes** that implement those interfaces.
- Returning `Task.CompletedTask` or `Task.FromResult` for async chains.
- Keeping DSL usage **fluent** and **ordered** (no lambdas or explicit delegates).
- Using **DI-friendly** constructors for drivers.
- Generating examples that **mirror the project’s style** and **naming conventions**.

### 🚫 Avoid
- SpecFlow-style `[Given]` / `[When]` / `[Then]` attributes.
- Gherkin text, regex step bindings, or external parsers.
- Reflection-based method dispatch.
- Introducing dependencies outside `Microsoft.Extensions.DependencyInjection` and `Castle.DynamicProxy`.
- Generic test frameworks scaffolding (`TestContext`, `ScenarioContext`, etc.) unless user code already imports it.

---

## 🧱 Example Structures

**Actor Interface:**
```csharp
[Actor("User")]
public interface IWhenUser
{
    Task Transfers(int amount);
}
```

**Driver Implementation:**
```csharp
public class WhenUserDriver : IWhenUser
{
    private readonly BankApiClient _client;

    public WhenUserDriver(BankApiClient client) => _client = client;

    public async Task Transfers(int amount)
        => await _client.PostAsync("/transfer", new { amount });
}
```

**Test Usage:**
```csharp
await When.User.Transfers(100).To.User("Alice");
```

---

## 🧩 Context Awareness

Copilot should recognize these folder roles:

| Folder | Description |
|---------|--------------|
| `FluentNetBDD/` | Core runtime library — DSL, proxy builder, interceptors. |
| `FluentNetBDD.Generation/` | Roslyn generation support types. |
| `FluentNetBDD.Generators/` | Source generator implementations (e.g., `DslGenerator`). |
| `FluentNetBDD.Tests/` | Example and integration tests demonstrating generated DSLs. |

---

## 💡 Copilot Intent Summary

- **Assist with DSL authoring**, not test logic.  
- **Respect type safety**, avoid dynamic or reflection-based shortcuts.  
- **Encourage composition and DI** for driver wiring.  
- **Mirror the style** of existing code (naming, structure, async patterns).  
- **Support progressive enhancement** — suggestions should feel like small, incremental improvements.

---

> The goal of these instructions is for Copilot to help developers **extend** FluentNetBDD, not to “recreate” a BDD framework.  
> Generated suggestions should feel like part of the library — fluent, typed, and composable.

