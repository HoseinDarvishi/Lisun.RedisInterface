using Lisun.RedisInterface.Abstraction;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Lisun.RedisInterface.Api.Controllers;

[ApiController]
public class PrefendController(
    IRedisService<Prefend> prefendCacheService,
    IRedisService<Refer> referCacheService) : Controller
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

        var refer = new Refer
        {
            Id = 200,
            Name = "TestRefer",
            PostalCode = "99032"
        };

        await prefendCacheService.SetAsync("90", prefend);
        await referCacheService.SetAsync("200", refer);

        return Ok();
    }

    [HttpGet("Get")]
    public async Task<IActionResult> Get()
    {
        var dis = await prefendCacheService.GetAsync("90");
        var dus = await referCacheService.GetAsync("200");
        return Ok(new { dis , dus });
    }
}
