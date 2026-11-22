using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Reflection;

namespace Lisun.RedisInterface.Abstraction;

public static class RedisInterface
{
    /// <summary>
    /// acivating redis + redisInterface
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="option">passing as lambda, 'RedisServiceLifeTime' has Scoped default value</param>
    /// <returns>services</returns>
    public static IServiceCollection AddRedisInterface(
        this IServiceCollection services , Action<RedisInterfaceOption> option = null)
    {
        var redisInterfaceOption = new RedisInterfaceOption();
        if(option != null)
            option(redisInterfaceOption);

        ValidateConfig(redisInterfaceOption);

        if (!redisInterfaceOption.UseSentinels)
        {
            // key

            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisInterfaceOption.ConnectionString)
            );
        }
        else
        {
            var sentinelConfig = new ConfigurationOptions
            {
                CommandMap = CommandMap.Sentinel,
                AbortOnConnectFail = false,
                Password = redisInterfaceOption.SentinelOption!.Password
            };
            foreach (var endpoint in redisInterfaceOption.SentinelOption.Endpoints)
            {
                sentinelConfig.EndPoints.Add(endpoint);
            }
            var sentinelConnection = ConnectionMultiplexer.Connect(sentinelConfig);
            var masterConfig = new ConfigurationOptions
            {
                ServiceName = redisInterfaceOption.SentinelOption.MasterName,
                Password = redisInterfaceOption.SentinelOption.Password,
                AbortOnConnectFail = false
            };
            var master = sentinelConnection
                .GetSentinelMasterConnection(masterConfig);

            services.AddSingleton<IConnectionMultiplexer>(master);
        }

        services.AddSingleton(redisInterfaceOption);

        ExtractConfigedClassCacheSettings();
        services.Add(
            ServiceDescriptor.Describe(typeof(IRedisService<>), typeof(RedisService<>), redisInterfaceOption.RedisServiceLifeTime));

        return services;
    }

    public static void RegisterConfig<CacheType>(CacheSetting setting)
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

    private static void ValidateConfig(RedisInterfaceOption opt)
    {
        ArgumentNullException.ThrowIfNull(opt);

        if (opt.UseSentinels)
        {
            ArgumentNullException.ThrowIfNull(opt.SentinelOption);

            if (string.IsNullOrWhiteSpace(opt.SentinelOption.MasterName))
                throw new ArgumentException("Sentinel MasterName is required.");

            if (opt.SentinelOption.Endpoints is null ||
                opt.SentinelOption.Endpoints.Count == 0)
                throw new ArgumentException("At least one Sentinel endpoint is required.");
        }
        else if (string.IsNullOrWhiteSpace(opt.ConnectionString))
        {
            throw new ArgumentException("Redis ConnectionString is required.");
        }
    }
}