namespace Lisun.RedisInterface.Abstraction;

public interface ICacheable
{
}

public interface IConfigedCacheable : ICacheable
{
    static abstract CacheSetting CacheSetting { get; }
}