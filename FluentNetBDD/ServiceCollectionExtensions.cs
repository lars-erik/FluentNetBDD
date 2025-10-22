using Microsoft.Extensions.DependencyInjection;

namespace FluentNetBDD;

public static class ServiceCollectionExtensions
{
    public static void AddScopedDriver<TDriver>(this IServiceCollection services)
        where TDriver : class
    {
        services.AddScoped<TDriver>();
        var interfaces = typeof(TDriver).GetInterfaces();
        foreach (var iface in interfaces)
        {
            services.AddScoped(iface, provider => provider.GetRequiredService<TDriver>());
        }
    }
}
