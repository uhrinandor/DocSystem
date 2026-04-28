using DocSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IratContract = DocSystem.TypeContracts.Irat;

namespace DocSystem.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class IratController(
    IratService iratService,
    ContractMapper contractMapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<IratContract>>> GetAll(CancellationToken ct)
    {
        var iratok = await iratService.GetAllAsync(ct);

        return Ok(iratok.Select(contractMapper.MapIrat).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IratContract>> GetById(Guid id, CancellationToken ct)
    {
        var irat = await iratService.GetByIdAsync(id, ct);

        if (irat is null)
            return NotFound();

        return Ok(contractMapper.MapIrat(irat));
    }
}
