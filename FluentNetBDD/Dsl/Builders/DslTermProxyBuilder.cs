using Castle.DynamicProxy;
using FluentNetBDD.Dsl.Interceptors;

namespace FluentNetBDD.Dsl.Builders;

public class DslTermProxyBuilder
{
    public static T Create<T>(string ownerName, IServiceProvider provider)
        where T : class
        => (T)Create(typeof(T), ownerName, provider);

    public static object Create(Type type, string ownerName, IServiceProvider provider, InterceptorStrategy strategy = InterceptorStrategy.DirectProperties)
    {
        var generator = new ProxyGenerator();
        var interceptors = new List<IInterceptor>();
        if (strategy.HasFlag(InterceptorStrategy.DirectProperties))
        {
            interceptors.Add(new PropertyImplementationsInterceptor(type, ownerName, provider));
        }
        if (strategy.HasFlag(InterceptorStrategy.InheritedMethods))
        {
            interceptors.Add(new InheritedMethodsInterceptor(type, ownerName, provider));
        }

        var proxy = generator.CreateInterfaceProxyWithoutTarget(
            type, 
            interceptors.ToArray()
        );

        return proxy;
    }
}

[Flags]
public enum InterceptorStrategy
{
    DirectProperties,
    InheritedMethods
}