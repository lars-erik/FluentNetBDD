using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace FluentNetBDD.Dsl.Interceptors;

public class PropertyImplementationsInterceptor<T> : IInterceptor
{
    private readonly Dictionary<MethodInfo, object> getters = new();

    public PropertyImplementationsInterceptor(IServiceProvider provider)
    {
        var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach(var prop in props)
        {
            var instance = provider.GetRequiredService(prop.PropertyType);
            if (prop.GetMethod != null)
            {
                getters.Add(prop.GetMethod!, instance);
            }
        }
    }

    public void Intercept(IInvocation invocation)
    {
        if (getters.TryGetValue(invocation.Method, out var instance))
        {
            invocation.ReturnValue = instance;
        }
    }
}