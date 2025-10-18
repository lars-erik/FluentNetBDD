# Copilot Project Instructions — DotNet.BDD

## Purpose
We are building a **BDD-style fluent DSL** for .NET with dynamically composable step interfaces.  
The system must allow Given/When/Then syntax to execute different test layers through DI and dynamic proxy composition.

---

## Key Architecture Rules

1. `Dsl<TGiven, TWhen, TThen>` is the root generic entry point for tests.  
   - It owns the DI container and exposes `Given`, `When`, and `Then` properties.  
   - Each property is a **proxy instance** built with Castle.DynamicProxy.  
   - Proxy interceptors delegate calls to implementations resolved from DI.

2. `Dsl.Register(IServiceCollection services)` should:  
   - Register step classes for each of the `Given`, `When`, and `Then` phases.  
   - Allow additional step interfaces to be discovered or registered manually.

3. The **proxy interceptor** must:  
   - Resolve the correct implementation from the DI container.  
   - Forward the method call (sync or async) to the resolved instance.  
   - Support multiple interfaces per phase (e.g., `IGivenUser`, `IGivenAccount`).

4. Tests must remain declarative and framework-independent.  
   - No test runner dependencies in the core library.  
   - Test frameworks (NUnit, xUnit, etc.) just call into the DSL.

---

## Style & Design Principles

- Target **.NET 8**, **C# 12**.  
- Prefer **interfaces** for step definitions.  
- Keep step implementations **stateless or per-scope**.  
- Use **records** for immutable data passed between steps.  
- Implement **async-aware interceptors** to support `Task` return types.  
- Avoid reflection-heavy scanning — rely on DI registration.  
- Code should feel like a **typed internal DSL**, not like SpecFlow/Gherkin parsing.

---

## Project structure

- Unit tests for the DSL core functionality in FluentNetBDD.Tests\DSL
- Implementation of all library code in FlutenNetBDD
- Example usage in FluentNetBDD.Tests\Examples

## Example Test Layout (Copilot Should Follow)

```
public class BankingFeature
{
    private Dsl<GivenForBank, WhenForBank, ThenForBank> Dsl;

    [SetUp]
    public void Setup()
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
}
```

---

## Copilot Should Generate

- ✅ `Dsl<TGiven, TWhen, TThen>` — the main class holding DI and proxies  
- ✅ `DynamicPhaseProxyFactory` — helper for creating `Given`, `When`, `Then` proxies  
- ✅ `StepInterceptor` — `IInterceptor` that resolves targets and forwards method calls (sync + async)  
- ✅ Example step interfaces and corresponding implementations  
- ✅ Minimal working NUnit example verifying dynamic delegation  

---

## Non-Goals

- ❌ No Gherkin text parsing  
- ❌ No automatic reflection-based step scanning  
- ❌ No external frameworks beyond Castle and Microsoft.Extensions.DependencyInjection  

---

By following this file, Copilot should generate **C# code** that enables **DI-driven, proxy-based BDD DSL syntax**, matching the test style shown above.
