using Lisun.RedisInterface.Abstraction;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Lisun.RedisInterface.Api.Controllers;

[ApiController]
public class PrefendController(IRedisService<Prefend> prefendCacheService) : Controller
{
    [HttpGet("Ref")]
    public async Task<IActionResult> Index()
    {
        var prefend = new Prefend
        {
            Id = 90,
            Name = "Test",
            PostalCode = "190239"
        };

        await prefendCacheService.SetAsync("90", prefend);

        return Ok();
    }

    [HttpGet("Get")]
    public async Task<IActionResult> Get()
    {
        var dis = await prefendCacheService.GetAsync("90");
        return Ok(dis);
    }
}
