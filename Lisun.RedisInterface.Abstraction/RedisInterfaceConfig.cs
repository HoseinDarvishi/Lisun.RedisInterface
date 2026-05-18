using Microsoft.Extensions.DependencyInjection;

namespace Lisun.RedisInterface.Abstraction;

public class RedisInterfaceOption
{
    public string Connection { get; set; } = default!;
    public string? InstanceName { get; set; }
    public ServiceLifetime RedisServiceLifeTime { get; set; } = ServiceLifetime.Scoped;
}