using Castle.DynamicProxy;
using FluentNetBDD.Dsl.Interceptors;

namespace FluentNetBDD.Dsl.Builders;

public class SubjunctionBuilder
{
    public static T Create<T>(IServiceProvider provider)
        where T : class
        => (T)Create(typeof(T), provider);

    public static object Create(Type type, IServiceProvider provider)
    {
        var generator = new ProxyGenerator();
        var proxy = generator.CreateInterfaceProxyWithoutTarget(type, new PropertyImplementationsInterceptor(type, provider));
        return proxy;
    }
}