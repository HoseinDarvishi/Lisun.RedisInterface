//using Lisun.RedisInterface.SentinelRepo;
using Microsoft.Extensions.DependencyInjection;

namespace Lisun.RedisInterface.Abstraction;

public class RedisInterfaceOption
{
    //public bool UseSentinels => SentinelOption is not null;
    public string InstanceName { get; set; }
    public string ConnectionString { get; set; }
    //public SentinelConfig? SentinelOption { get; set; }
    public ServiceLifetime RedisServiceLifeTime { get; set; } = ServiceLifetime.Scoped;
}