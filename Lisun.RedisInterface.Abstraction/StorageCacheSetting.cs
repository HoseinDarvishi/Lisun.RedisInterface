using Lisun.RedisInterface.Abstraction;

namespace Lisun.RedisInterface;

public static class Storage
{
    private static readonly Dictionary<Type, CacheSetting> _settings = new();
    public static IReadOnlyDictionary<Type, CacheSetting> Settings => _settings;

    internal static void Register<T>(CacheSetting setting)
        where T : ICacheable
    {
        var type = typeof(T);

        if (_settings.ContainsKey(type))
            throw new InvalidOperationException($"CacheSetting for {type.Name} already registered");

        _settings[type] = setting;
    }

    internal static void Register(Type type, CacheSetting setting)
    {
        if (_settings.ContainsKey(type))
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
