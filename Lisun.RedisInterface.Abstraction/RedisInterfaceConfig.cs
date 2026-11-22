using Microsoft.Extensions.DependencyInjection;

namespace Lisun.RedisInterface.Abstraction;

public class RedisInterfaceOption
{
    public bool UseSentinels { get; set; }
    public string InstanceName { get; set; }
    public string ConnectionString { get; set; }
    public SentinelConfig? SentinelOption { get; set; }
    public ServiceLifetime RedisServiceLifeTime { get; set; } = ServiceLifetime.Scoped;
}

public class SentinelConfig
{
    public string MasterName { get; set; }
    public string Password { get; set; }
    public List<string> Endpoints { get; set; }
}