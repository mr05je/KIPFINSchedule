using Microsoft.AspNetCore.Mvc;

namespace KIPFINSchedule.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class UselessController : ControllerBase
{
    [HttpGet("useless")]
    public IActionResult Useless()
    {
        return Ok("Ok!");
    }
}