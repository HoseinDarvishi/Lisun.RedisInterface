using Lisun.RedisInterface.Abstraction;

namespace Lisun.RedisInterface.Api
{
    public class Prefend : IConfigedCacheable
    {
        public static CacheSetting CacheSetting 
            => new CacheSetting("Prefend").ExpireAfter(TimeSpan.FromMinutes(2));

        public int Id { get; set; }
        public string Name { get; set; }
        public string PostalCode { get; set; }
    }

    public class Refer : ICacheable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PostalCode { get; set; }
    }
}
