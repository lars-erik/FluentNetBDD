using System.Diagnostics;
using System.Reflection;
using Castle.DynamicProxy;
using FluentNetBDD.Dsl.Builders;

namespace FluentNetBDD.Dsl.Interceptors;

public class PropertyImplementationsInterceptor : IInterceptor
{
    private readonly string ownerName;
    private readonly Dictionary<MethodInfo, object> getters = new();

    public PropertyImplementationsInterceptor(Type type, string ownerName, IServiceProvider provider)
    {
        this.ownerName = ownerName;
        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var prop in props)
        {
            var instance = provider.GetService(prop.PropertyType);

            if (instance == null && type.IsInterface)
            {
                instance = DslTermProxyBuilder.Create(prop.PropertyType, $"{ownerName} {prop.Name}", provider, InterceptorStrategy.DirectProperties | InterceptorStrategy.InheritedMethods);
            }

            if (instance == null)
            {
                throw new Exception($"Could not create interceptor for type {prop.PropertyType.Name} in {type.Name}.{prop.Name}");
            }

            if (prop.GetMethod != null)
            {
                getters.Add(prop.GetMethod!, instance);
            }
            else
            {
                throw new Exception($"Can't set up DSL property {type.Name}.{prop.Name} since it does not have a setter.");
            }
        }
    }

    public void Intercept(IInvocation invocation)
    {
        if (getters.TryGetValue(invocation.Method, out var instance))
        {
            invocation.ReturnValue = instance;
        }
        else
        {
            invocation.Proceed();
        }
    }
}

public class PropertyImplementationsInterceptor<T>(string ownerName, IServiceProvider provider)
    : PropertyImplementationsInterceptor(typeof(T), ownerName, provider);