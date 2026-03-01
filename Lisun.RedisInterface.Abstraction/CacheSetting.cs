using System.Text.Json;

namespace Lisun.RedisInterface.Abstraction;

public sealed class CacheSetting 
{
    public CacheSetting(string areaPrefix)
    {
        AreaPrefix = areaPrefix;
    }

    public string AreaPrefix { get; private set; }
    public TimeSpan? RelativeExpireTime { get; private set; }
    public DateTimeOffset? AbsoluteExpireDateTime { get; private set; }
    public TimeSpan? SlidingExpiration { get; private set; }
    public JsonSerializerOptions? JsonOption { get; private set; }

    public CacheSetting ExpireAfter(TimeSpan timeSpan)
    {
        RelativeExpireTime = timeSpan;
        ValidateExpirationConfig();
        return this;
    }

    public CacheSetting ExpireAt(DateTimeOffset dateTimeOffset)
    {
        AbsoluteExpireDateTime = dateTimeOffset;
        ValidateExpirationConfig();
        return this;
    }

    public CacheSetting SetSlidingExpiration(TimeSpan timeSpan)
    {
        SlidingExpiration = timeSpan;
        return this;
    }

    public CacheSetting SetJsonOption(JsonSerializerOptions option)
    {
        JsonOption = option;
        return this;
    }


    private void ValidateExpirationConfig()
    {
        if (RelativeExpireTime.HasValue && AbsoluteExpireDateTime.HasValue)
            throw new InvalidOperationException("RelativeExpireTime and AbsoluteExpireDateTime could not has value together");
    }
}
