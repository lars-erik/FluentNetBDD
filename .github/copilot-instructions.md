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
   - Must expose a constructor overload that accepts an `IServiceProvider` (or `IServiceCollection`) and builds the proxies at construction time so tests can use `new Dsl<IGivenX>(provider)` without reflection.
   - The constructor should delegate proxy creation to the project's proxy factory (e.g., `DynamicPhaseProxyFactory` or `SubjunctionBuilder.Create<T>`).

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
   - Tests should instantiate the DSL using the public API (e.g. `new Dsl<IGivenUserWithName>(provider)`) rather than use reflection to mutate private state.
   - Example test usage to drive implementation:

```
var dsl = new Dsl<IGivenUserWithName>(provider);
dsl.Given.User.WithName("Trinity");
Assert.That(dsl.Given.User.Name, Is.EqualTo("Trinity"));
```

---

## Style & Design Principles

- Target **.NET 9**, **C# 13** (or detect the repository TFM and prefer that).  
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

---

## Edit-scope rules for the assistant

Short, enforceable rules for when the assistant may apply edits automatically vs when it must propose a plan and wait for approval.

- Default: Do NOT apply code edits unless the user explicitly approves a proposed plan.
- Automatic edits allowed without prior approval when ALL of the following are true:
  - The user asked for `"Make minimal edits"` or an equivalent short phrasing.
  - The total changes are <= 15 lines across all files.
  - No new classes, records, interfaces, or projects are added outside the currently discussed scope.
- Approval required (assistant must present a short plan and wait for user to reply `"Apply changes"`) when ANY of the following is true:
  - The edit set exceeds 15 lines across all files.
  - New classes/records/interfaces/types are added outside the defined scope for the current task.
  - Files outside the explicitly mentioned files are modified.
- Exceeding the 15-line automatic limit is allowed only when the user explicitly instructs the assistant to do so. Example phrases that grant permission:
  - `"You may exceed 16 lines for this change."`
  - `"Allow edits >15 lines for this task."`
  - `"Apply changes (allow larger edits)"`
  In such cases the assistant may proceed without requiring an additional confirmation.
- When presenting a plan the assistant should include:
  - A one-line summary of intent.
  - The list of files to change with exact modification summaries (e.g., `add method X to file Y`, `change 3 lines in Z`).
  - An estimated line-count of the edits.
- When edits are applied the assistant should:
  - Run a build and report the result (ask for permission if the user didn't initiate the change in the same session).
  - If tests are run, clearly report failures only; keep output terse.

### How users should phrase requests to control behavior

- `Propose changes` — assistant must only return a short plan/diff summary and wait.
- `Apply changes` — assistant may edit files, but must follow the 15-line rule unless the user also specifies `Allow edits >15 lines`.
- `Make minimal edits` — assistant may auto-apply edits up to 15 lines.
- `You may exceed 16 lines` — assistant may perform larger edits without extra confirmation.

If you want stricter limits, change the `15` number to your preferred threshold.
