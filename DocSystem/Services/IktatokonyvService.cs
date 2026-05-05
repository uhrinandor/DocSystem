using DocSystem.Configuration;
using DocSystem.Dtos;
using DocSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DocSystem.Services;

public class IktatokonyvService(
    AppDbContext db,
    ILogger<IktatokonyvService> logger)
{
    public async Task<Iktatokonyv> CreateAsync(CreateIktatokonyvDto dto, CancellationToken ct = default)
    {
        var iktatokonyv = new Iktatokonyv
        {
            Nev = dto.Nev,
            Kod = dto.Kod,
            Evszam = dto.Evszam
        };

        var foszamCounter = new FoszamCounter
        {
            Iktatokonyv = iktatokonyv,
            Next = 0
        };

        iktatokonyv.FoszamCounter = foszamCounter;

        db.Iktatokonyvek.Add(iktatokonyv);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Iktatokonyv created successfully. IktatokonyvId: {IktatokonyvId}; Kod: {Kod}; Evszam: {Evszam}; Nev: {Nev}",
            iktatokonyv.Id,
            iktatokonyv.Kod,
            iktatokonyv.Evszam,
            iktatokonyv.Nev);

        return iktatokonyv;
    }

    public async Task<List<Iktatokonyv>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Iktatokonyvek
            .AsNoTracking()
            .Include(x => x.Iktatoszamok)
            .ToListAsync(ct);
    }

    public async Task<Iktatokonyv?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Iktatokonyvek
            .AsNoTracking()
            .Include(x => x.Iktatoszamok)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
    }
}
