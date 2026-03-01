using Lisun.RedisInterface.Abstraction;

namespace Lisun.RedisInterface.Api
{
    public class Prefend : ICacheable
    {
        public static CacheSetting CacheSetting 
            => new CacheSetting("Prefend").ExpireAfter(TimeSpan.FromMinutes(2));

        public int Id { get; set; }
        public string Name { get; set; }
        public string PostalCode { get; set; }
    }
}
