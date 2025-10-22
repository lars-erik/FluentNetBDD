using System.Runtime.CompilerServices;
using FluentNetBDD.Dsl;

namespace FluentNetBDD.Tests.Common;

public static class VerifyExtensions
{
    public static async Task Verify(this DslState dslState, [CallerFilePath] string callerPath = "")
    {
        await Verifier.Verify(dslState.ToString())
            .UseDirectory("Approvals");
    }
}
