using DocSystem.Dtos;
using DocSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await authService.Login(dto, ct);

        if (result is null)
            return Unauthorized();

        return Ok(result);
    }
}
