using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Lisun.RedisInterface.Abstraction;

public sealed class RedisService<CacheType> : IRedisService<CacheType>
    where CacheType : ICacheable
{
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _defaultJsonOption;
    private readonly CacheSetting _setting;

    public RedisService(IDistributedCache cahce)
    {
        _cache = cahce;
        _setting = Storage.Get<CacheType>();
        _defaultJsonOption = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<CacheType?> GetAsync(string key, CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(BuildKey(key), ct);
        return Deserialize(bytes);
    }

    public async Task RemoveAsync(string key,CancellationToken ct = default)
        => await _cache.RemoveAsync(BuildKey(key), ct);

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(BuildKey(key), ct);
        return bytes is not null;
    }

    public async Task<CacheType> GetOrSetAsync(
        string key,
        Func<CancellationToken, Task<CacheType>> factory,
        CancellationToken ct = default)
    {
        var exist = await GetAsync(key, ct);
        if (exist is not null)
            return exist;

        var value = await factory(ct)
            ?? throw new InvalidOperationException("Factory returned null.");

        await SetAsync(key,value, ct);
        return value;
    }

    public async Task SetAsync(
        string key,
        CacheType value,
        CancellationToken ct = default)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var bytes = JsonSerializer.SerializeToUtf8Bytes(value,JsonOption());
        await _cache.SetAsync(BuildKey(key), bytes, CacheEntryOption(), ct);
    }

    private string BuildKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        return $"{_setting.AreaPrefix}:{key}"; 
    }

    private JsonSerializerOptions JsonOption()
        => _setting.JsonOption ?? _defaultJsonOption;

    private DistributedCacheEntryOptions CacheEntryOption()
        =>  new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _setting.RelativeExpireTime,
            AbsoluteExpiration = _setting.AbsoluteExpireDateTime,
            SlidingExpiration = _setting.SlidingExpiration
        };

    private CacheType? Deserialize(byte[]? bytes)
        =>  JsonSerializer.Deserialize<CacheType>(bytes, JsonOption());
}