using DocSystem.Dtos;
using DocSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserContract = DocSystem.TypeContracts.User;

namespace DocSystem.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController(
    UserService userService,
    ContractMapper contractMapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserContract>>> GetAll(CancellationToken ct)
    {
        var users = await userService.GetAllAsync(ct);

        return Ok(users.Select(contractMapper.MapUser).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<UserContract>> Create([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        var user = await userService.CreateAsync(dto, ct);

        return Ok(contractMapper.MapUser(user));
    }
}
