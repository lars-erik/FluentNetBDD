using Castle.DynamicProxy;
using FluentNetBDD.Dsl.Interceptors;

namespace FluentNetBDD.Dsl.Builders;

public class SubjunctionBuilder
{
    public static T Create<T>(IServiceProvider provider)
        where T : class
    {
        var generator = new ProxyGenerator();
        var proxy = generator.CreateInterfaceProxyWithoutTarget<T>(new PropertyImplementationsInterceptor<T>(provider));
        return proxy;
    }
}