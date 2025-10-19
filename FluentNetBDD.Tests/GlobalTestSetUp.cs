using NUnit.Framework.Diagnostics;
using System.Diagnostics;

namespace FluentNetBDD.Tests;

[SetUpFixture]
public class GlobalTestSetUp
{
    [OneTimeSetUp]
    public void SetUpTraceListener()
    {
        Trace.Listeners.Add(new ProgressTraceListener());
    }
}
