using DocSystem.Configuration;
using DocSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DocSystem.Services;

public class IratService(AppDbContext db)
{
    public async Task<List<Irat>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Iratok
            .AsNoTracking()
            .Include(x => x.Iktatoszam)
            .ThenInclude(x => x.Iktatokonyv)
            .Include(x => x.Ugyirat)
            .ToListAsync(ct);
    }

    public async Task<Irat?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Iratok
            .AsNoTracking()
            .Include(x => x.Iktatoszam)
            .ThenInclude(x => x.Iktatokonyv)
            .Include(x => x.Ugyirat)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
    }
}
