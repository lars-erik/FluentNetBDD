using NUnit.Framework.Diagnostics;
using System.Diagnostics;
using DiffEngine;

namespace FluentNetBDD.Tests;

[SetUpFixture]
public class GlobalTestSetUp
{
    [OneTimeSetUp]
    public void SetUpGlobals()
    {
        Trace.Listeners.Add(new ProgressTraceListener());

        DiffTools.UseOrder(DiffTool.VisualStudio, DiffTool.VisualStudioCode);
    }
}
