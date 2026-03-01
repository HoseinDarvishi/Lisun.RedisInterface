using Microsoft.Extensions.DependencyInjection;

namespace Lisun.RedisInterface.Abstraction;

public static class RedisRegisterExtensions
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
        services.AddScoped(typeof(IRedisService<>), typeof(RedisService<>));
        return services;
    }
}