using DocSystem.Configuration;
using DocSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DocSystem.Services;

public class UgyiratService(AppDbContext db)
{
    public async Task<List<Ugyirat>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Ugyiratok
            .AsNoTracking()
            .Include(x => x.Iktatoszam)
            .ThenInclude(x => x.Iktatokonyv)
            .Include(x => x.Irats)
            .ToListAsync(ct);
    }

    public async Task<Ugyirat?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Ugyiratok
            .AsNoTracking()
            .Include(x => x.Iktatoszam)
            .ThenInclude(x => x.Iktatokonyv)
            .Include(x => x.Irats)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
    }
}
