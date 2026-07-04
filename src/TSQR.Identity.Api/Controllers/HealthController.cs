using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TSQR.Identity.Api.Controllers;

[ApiController]
[Route("api")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok", service = "tsqr-identity", time = DateTime.UtcNow });
}
