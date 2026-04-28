using DocSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UgyiratContract = DocSystem.TypeContracts.Ugyirat;

namespace DocSystem.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UgyiratController(
    UgyiratService ugyiratService,
    ContractMapper contractMapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UgyiratContract>>> GetAll(CancellationToken ct)
    {
        var ugyiratok = await ugyiratService.GetAllAsync(ct);

        return Ok(ugyiratok.Select(contractMapper.MapUgyirat).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UgyiratContract>> GetById(Guid id, CancellationToken ct)
    {
        var ugyirat = await ugyiratService.GetByIdAsync(id, ct);

        if (ugyirat is null)
            return NotFound();

        return Ok(contractMapper.MapUgyirat(ugyirat));
    }
}
