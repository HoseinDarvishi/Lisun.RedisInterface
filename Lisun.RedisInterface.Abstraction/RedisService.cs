using StackExchange.Redis;
using System.Text.Json;

namespace Lisun.RedisInterface.Abstraction;

public sealed class RedisService<CacheType> : IRedisService<CacheType>
    where CacheType : ICacheable
{
    private readonly IDatabase _database;
    private readonly JsonSerializerOptions _defaultJsonOption;
    private readonly CacheSetting _setting;
    private readonly string _instanceName;

    public RedisService(
        IConnectionMultiplexer multiplexer,
        RedisInterfaceOption option)
    {
        ArgumentNullException.ThrowIfNull(multiplexer);
        ArgumentNullException.ThrowIfNull(option);

        _setting = Storage.Get<CacheType>();

        ValidateCacheSetting(_setting);

        _instanceName = string.IsNullOrWhiteSpace(option.InstanceName)
            ? string.Empty
            : $"{option.InstanceName}:";

        _database = multiplexer.GetDatabase();

        _defaultJsonOption = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<CacheType?> GetAsync(
        string key,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var redisKey = BuildKey(key);

        var value = await _database.StringGetAsync(redisKey);

        if (value.IsNull)
            return default;

        if (_setting.SlidingExpiration.HasValue)
            await _database.KeyExpireAsync(
                redisKey,
                _setting.SlidingExpiration.Value);

        return Deserialize((byte[])value);
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await _database.KeyDeleteAsync(BuildKey(key));
    }

    public async Task<bool> ExistsAsync(
        string key,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return await _database.KeyExistsAsync(BuildKey(key));
    }

    public async Task<CacheType> GetOrSetAsync(
        string key,
        Func<CancellationToken, Task<CacheType>> factory,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var exist = await GetAsync(key, ct);

        if (exist is not null)
            return exist;

        var value = await factory(ct)
            ?? throw new InvalidOperationException("Factory returned null.");

        await SetAsync(key, value, ct);

        return value;
    }

    public async Task SetAsync(
        string key,
        CacheType value,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(value);

        ct.ThrowIfCancellationRequested();

        var bytes = JsonSerializer.SerializeToUtf8Bytes(
            value,
            JsonOption());

        await _database.StringSetAsync(
            BuildKey(key),
            bytes,
            GetExpiration());
    }



    private string BuildKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException(
                "Cache key cannot be null or empty.",
                nameof(key));

        return $"{_instanceName}{_setting.AreaPrefix}:{key}";
    }

    private JsonSerializerOptions JsonOption()
        => _setting.JsonOption ?? _defaultJsonOption;

    private TimeSpan? GetExpiration()
        => _setting.SlidingExpiration ?? _setting.RelativeExpireTime;

    private void ValidateCacheSetting(CacheSetting setting)
    {
        ArgumentNullException.ThrowIfNull(setting);

        if (setting.RelativeExpireTime.HasValue &&
            setting.RelativeExpireTime.Value <= TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                "RelativeExpireTime must be greater than zero.");
        }

        if (setting.SlidingExpiration.HasValue &&
            setting.SlidingExpiration.Value <= TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                "SlidingExpiration must be greater than zero.");
        }

        if (setting.RelativeExpireTime.HasValue &&
            setting.SlidingExpiration.HasValue)
        {
            throw new InvalidOperationException(
                "RelativeExpireTime and SlidingExpiration cannot be used together.");
        }
    }

    private CacheType? Deserialize(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
            return default;

        return JsonSerializer.Deserialize<CacheType>(
            bytes,
            JsonOption());
    }
}