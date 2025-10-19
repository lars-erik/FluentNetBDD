using Castle.DynamicProxy;

namespace FluentNetBDD.Dsl.Interceptors;

internal class InheritedMethodsInterceptor : IInterceptor
{
    private readonly Type type;
    private readonly Dictionary<Type, object> instances = new();
    private readonly IServiceProvider provider;

    public InheritedMethodsInterceptor(Type type, IServiceProvider provider)
    {
        this.type = type;
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
        if (invocation.Method.DeclaringType != null && instances.TryGetValue(invocation.Method.DeclaringType, out var instance))
        {
            invocation.ReturnValue = invocation.Method.Invoke(instance, invocation.Arguments);
        }
        else
        {
            invocation.Proceed();
        }
    }
}

internal class InheritedMethodsInterceptor<T>(Type type, IServiceProvider provider)
    : InheritedMethodsInterceptor(type, provider);