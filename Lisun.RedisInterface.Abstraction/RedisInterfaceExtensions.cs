using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime;

namespace Lisun.RedisInterface.Abstraction;

public static class RedisInterfaceExtensions
{
    public static IServiceCollection AddRedisInterface(
        this IServiceCollection services, Action<RedisInterfaceConfig> config)
    {
        var redisConfig = new RedisInterfaceConfig();
        config(redisConfig);

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfig.Configuration;
            options.InstanceName = redisConfig.InstanceName;
        });
        ExtractConfigedClassCacheSettings();
        services.AddScoped(typeof(IRedisService<>), typeof(RedisService<>));
        return services;
    }

    public static void RegisterCacheSetting<CacheType>(CacheSetting setting)
        where CacheType : ICacheable
    {
        if (setting is null)
            throw new ArgumentNullException("CacheSetting is null !");
        Storage.Register<CacheType>(setting);
    }

    private static void ExtractConfigedClassCacheSettings()
    {
        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !IsSystemAssembly(a));

        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (type is null || !type.IsClass || type.IsAbstract || !typeof(IConfigedCacheable).IsAssignableFrom(type))
                    continue;

                var property = type.GetProperty(
                    "CacheSetting",
                    BindingFlags.Public | BindingFlags.Static);

                if (property == null)
                    throw new InvalidOperationException($"Type {type.Name} implements IConfigedCacheable but is missing the static CacheSetting property.");

                var value = property.GetValue(null) as CacheSetting;

                if (value == null)
                    throw new InvalidOperationException($"The static property CacheSetting on type {type.Name} has not been assigned a value. Please initialize it.");

                Storage.Register(type,value);
            }
        }
    }

    private static bool IsSystemAssembly(this Assembly assembly)
    {
        var name = assembly.GetName().Name;
        return name!.StartsWith("System") || name.StartsWith("Microsoft") || name.StartsWith("mscorlib") || name.StartsWith("netstandard");
    }
}