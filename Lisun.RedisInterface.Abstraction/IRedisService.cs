namespace Lisun.RedisInterface.Abstraction;

public interface IRedisService<CacheType> where CacheType : ICacheable
{
    Task SetAsync(string key ,CacheType value, CancellationToken ct = default);
    Task<CacheType?> GetAsync(string key, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task<bool> ExistsAsync(string key,CancellationToken ct = default);
}