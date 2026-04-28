using DocSystem.Dtos;
using DocSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IktatokonyvContract = DocSystem.TypeContracts.Iktatokonyv;

namespace DocSystem.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class IktatokonyvController(
    IktatokonyvService iktatokonyvService,
    ContractMapper contractMapper) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<IktatokonyvContract>> Create(
        [FromBody] CreateIktatokonyvDto dto,
        CancellationToken ct)
    {
        var iktatokonyv = await iktatokonyvService.CreateAsync(dto, ct);

        return Ok(contractMapper.MapIktatokonyv(iktatokonyv));
    }

    [HttpGet]
    public async Task<ActionResult<List<IktatokonyvContract>>> GetAll(CancellationToken ct)
    {
        var iktatokonyvek = await iktatokonyvService.GetAllAsync(ct);

        return Ok(iktatokonyvek.Select(contractMapper.MapIktatokonyv).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IktatokonyvContract>> GetById(Guid id, CancellationToken ct)
    {
        var iktatokonyv = await iktatokonyvService.GetByIdAsync(id, ct);

        if (iktatokonyv is null)
            return NotFound();

        return Ok(contractMapper.MapIktatokonyv(iktatokonyv));
    }
}
