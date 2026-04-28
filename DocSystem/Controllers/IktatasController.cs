using DocSystem.Dtos;
using DocSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IratContract = DocSystem.TypeContracts.Irat;
using UgyiratContract = DocSystem.TypeContracts.Ugyirat;

namespace DocSystem.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class IktatasController(
    IktatasService iktatasService,
    ContractMapper contractMapper) : ControllerBase
{
    [HttpPost("foszamos")]
    public async Task<ActionResult<UgyiratContract>> FoszamosIktatas(
        [FromBody] FoszamosIktatasDto dto,
        CancellationToken ct)
    {
        var ugyirat = await iktatasService.FoszamosIktatas(dto, ct);

        return Ok(contractMapper.MapUgyirat(ugyirat));
    }

    [HttpPost("alszamos")]
    public async Task<ActionResult<IratContract>> AlszamosIktatas(
        [FromBody] AlszamosIktatasDto dto,
        CancellationToken ct)
    {
        var irat = await iktatasService.AlszamosIktatas(dto, ct);

        return Ok(contractMapper.MapIrat(irat));
    }
}
