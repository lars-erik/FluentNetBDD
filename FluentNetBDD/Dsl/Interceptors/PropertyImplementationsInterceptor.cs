using System.Reflection;
using Castle.DynamicProxy;
using FluentNetBDD.Dsl.Builders;

namespace FluentNetBDD.Dsl.Interceptors;

public class PropertyImplementationsInterceptor : IInterceptor
{
    private readonly Dictionary<MethodInfo, object> getters = new();

    public PropertyImplementationsInterceptor(Type type, IServiceProvider provider)
    {
        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var prop in props)
        {
            var instance = provider.GetService(prop.PropertyType);

            if (instance == null && type.IsInterface)
            {
                instance = SubjunctionBuilder.Create(prop.PropertyType, provider);
            }

            if (instance == null)
            {
                throw new Exception($"Could not create interceptor for type {type}");
            }

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

public class PropertyImplementationsInterceptor<T>(IServiceProvider provider)
    : PropertyImplementationsInterceptor(typeof(T), provider);