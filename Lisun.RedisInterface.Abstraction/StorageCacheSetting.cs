using Lisun.RedisInterface.Abstraction;
using System.Collections.Concurrent;

namespace Lisun.RedisInterface;

public static class Storage
{
    private static readonly ConcurrentDictionary<Type, CacheSetting> _settings = new();
    public static IReadOnlyDictionary<Type, CacheSetting> Settings => _settings;

    internal static void Register<T>(CacheSetting setting)
        where T : ICacheable
    {
        ArgumentNullException.ThrowIfNull(setting);

        if (!_settings.TryAdd(typeof(T), setting))
            throw new InvalidOperationException(
                $"CacheSetting for {typeof(T).Name} already registered.");
    }

    internal static void Register(Type type, CacheSetting setting)
    {
        ArgumentNullException.ThrowIfNull(setting);

        if (_settings.TryAdd(type,setting))
            throw new InvalidOperationException($"CacheSetting for {type.Name} already registered");

        _settings[type] = setting;
    }

    public static CacheSetting Get<T>()
        where T : ICacheable
    {
        _settings.TryGetValue(typeof(T), out var setting);
        if (setting is null)
            throw new ArgumentNullException($"setting for type {typeof(T).Name} not registered");
        return setting;
    }
}
