using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RooviApp.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromQuery] string username)
    {

        return Ok();
    }
}
