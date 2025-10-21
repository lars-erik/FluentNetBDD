using System.Diagnostics;
using Castle.DynamicProxy;
using BindingFlags = System.Reflection.BindingFlags;

namespace FluentNetBDD.Dsl.Interceptors;

internal class InheritedMethodsInterceptor : IInterceptor
{
    private readonly Type type;
    private readonly string ownerName;
    private readonly Dictionary<Type, object> instances = new();
    private readonly IServiceProvider provider;

    public InheritedMethodsInterceptor(Type type, string ownerName, IServiceProvider provider)
    {
        this.type = type;
        this.ownerName = ownerName;
        this.provider = provider;

        var inheritedInterfaces = type.GetInterfaces();
        foreach (var inheritedInterface in inheritedInterfaces)
        {
            var instance = provider.GetService(inheritedInterface);
            if (instance != null)
            {
                instances[inheritedInterface] = instance;
            }
            else
            {
                throw new Exception($"Could not find a real instance to delegate to for {type.Name} : {inheritedInterface.FullName}");
            }
        }
    }

    public void Intercept(IInvocation invocation)
    {
        // TODO: Logging to an internal "buffer" so we can use "printers"?
        // TODO: Reflect argument names and type around? Decorate for logging?
        if (invocation.Method.DeclaringType != null && instances.TryGetValue(invocation.Method.DeclaringType, out var instance))
        {
            var addArguments = invocation.Arguments.Any();
            var argumentList = $"({ String.Join(", ", invocation.Arguments.Select(x => x?.ToString() ?? "null"))})";
            var logLine = $"{ownerName} {invocation.Method.Name} {(addArguments ? argumentList : "")}";
            try
            {
                invocation.ReturnValue = invocation.Method.Invoke(instance, invocation.Arguments);
                Trace.WriteLine("✅ " + logLine);
            }
            catch
            {
                Trace.WriteLine("❌ " + logLine);
                throw;
            }

            // Ensure we always return proxy for chaining
            if (invocation.ReturnValue == instance)
            {
                invocation.ReturnValue = invocation.Proxy;
            }
        }
        else
        {
            invocation.Proceed();
        }
    }
}

internal class InheritedMethodsInterceptor<T>(Type type, string ownerName, IServiceProvider provider)
    : InheritedMethodsInterceptor(type, ownerName, provider);