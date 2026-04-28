using DocSystem.Configuration;
using DocSystem.Dtos;
using DocSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DocSystem.Services;

public class IktatasService(
    AppDbContext _db,
    ILogger<IktatasService> logger)
{
    public async Task<Ugyirat> FoszamosIktatas(FoszamosIktatasDto dto, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Starting foszamos iktatas. IktatokonyvId: {IktatokonyvId}; Subject: {Subject}",
            dto.IktatokonyvId,
            dto.Subject);

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var iktatokonyv = await _db.Iktatokonyvek
            .SingleAsync(x => x.Id == dto.IktatokonyvId, ct);

        var iktatoszam = await ReserveFoszamAsync(iktatokonyv, ct);

        var ugyirat = new Ugyirat
        {
            Iktatoszam = iktatoszam,
            Irats = new List<Irat>()
        };

        var irat = new Irat
        {
            Subject = dto.Subject,
            Details = dto.Details,
            Iktatoszam = iktatoszam,
            Ugyirat = ugyirat
        };

        ugyirat.Irats.Add(irat);
        _db.Iratok.Add(irat);
        _db.Ugyiratok.Add(ugyirat);

        MarkValid(iktatoszam, ct);

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        logger.LogInformation(
            "Foszamos iktatas completed successfully. UgyiratId: {UgyiratId}; IratId: {IratId}; IktatoszamId: {IktatoszamId}; Foszam: {Foszam}; Alszam: {Alszam}; IktatokonyvId: {IktatokonyvId}",
            ugyirat.Id,
            irat.Id,
            iktatoszam.Id,
            iktatoszam.Foszam,
            iktatoszam.Alszam,
            iktatoszam.IktatokonyvId);

        return ugyirat;
    }

    public async Task<Irat> AlszamosIktatas(AlszamosIktatasDto dto, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Starting alszamos iktatas. UgyiratId: {UgyiratId}; Subject: {Subject}",
            dto.UgyiratId,
            dto.Subject);

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var ugyirat = await _db.Ugyiratok
            .Include(x => x.Iktatoszam)
            .ThenInclude(x => x.Iktatokonyv)
            .SingleAsync(x => x.Id == dto.UgyiratId, ct);

        var iktatoszam = await ReserveAlszamAsync(ugyirat.Iktatoszam, ct);

        var irat = new Irat
        {
            Subject = dto.Subject,
            Details = dto.Details,
            Iktatoszam = iktatoszam,
            Ugyirat = ugyirat
        };

        _db.Iratok.Add(irat);

        MarkValid(iktatoszam, ct);

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        logger.LogInformation(
            "Alszamos iktatas completed successfully. UgyiratId: {UgyiratId}; IratId: {IratId}; IktatoszamId: {IktatoszamId}; Foszam: {Foszam}; Alszam: {Alszam}; IktatokonyvId: {IktatokonyvId}",
            ugyirat.Id,
            irat.Id,
            iktatoszam.Id,
            iktatoszam.Foszam,
            iktatoszam.Alszam,
            iktatoszam.IktatokonyvId);

        return irat;
    }

    public async Task<Iktatoszam> ReserveFoszamAsync(Iktatokonyv iktatokonyv, CancellationToken ct  = default)
    {
        var nextValues = await _db.Database
            .SqlQuery<int>(
                $"""
                 UPDATE "FoszamCounters"
                 SET "Next" = "Next" + 1
                 WHERE "IktatokonyvId" = {iktatokonyv.Id}
                 RETURNING "Next"
                 """
            )
            .ToListAsync(ct);

        var next = nextValues.Single();

        var uj = new Iktatoszam
        {
            IktatokonyvId = iktatokonyv.Id,
            Iktatokonyv = iktatokonyv,
            Foszam = next,
            Alszam = 1
        };

        _db.Iktatoszamok.Add(uj);
        await _db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Reserved foszam successfully. IktatoszamId: {IktatoszamId}; IktatokonyvId: {IktatokonyvId}; Kod: {Kod}; Evszam: {Evszam}; Foszam: {Foszam}; Alszam: {Alszam}",
            uj.Id,
            iktatokonyv.Id,
            iktatokonyv.Kod,
            iktatokonyv.Evszam,
            uj.Foszam,
            uj.Alszam);

        return uj;
    }

    public async Task<Iktatoszam> ReserveAlszamAsync(Iktatoszam iktatoszam, CancellationToken ct = default)
    {
        var nextValues = await _db.Database
            .SqlQuery<int>($"""
                                UPDATE "AlszamCounters"
                                SET "Next" = "Next" + 1
                                WHERE "IktatoszamId" = {iktatoszam.Id}
                                RETURNING "Next"
                            """)
            .ToListAsync(ct);

        var next = nextValues.Single();

        var entity = new Iktatoszam
        {
            IktatokonyvId = iktatoszam.IktatokonyvId,
            Iktatokonyv = iktatoszam.Iktatokonyv,
            Foszam = iktatoszam.Foszam,
            Alszam = next
        };

        _db.Iktatoszamok.Add(entity);

        await _db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Reserved alszam successfully. IktatoszamId: {IktatoszamId}; ParentIktatoszamId: {ParentIktatoszamId}; IktatokonyvId: {IktatokonyvId}; Foszam: {Foszam}; Alszam: {Alszam}",
            entity.Id,
            iktatoszam.Id,
            entity.IktatokonyvId,
            entity.Foszam,
            entity.Alszam);

        return entity;
    }

    public void MarkValid(Iktatoszam iktatoszam, CancellationToken cancellationToken = default)
    {
        iktatoszam.Valid = true;
        if (iktatoszam.Alszam == 1)
        {
            var counter = new AlszamCounter()
            {
                Iktatoszam = iktatoszam,
                Next = 1
            };
            _db.AlszamCounters.Add(counter);

            logger.LogInformation(
                "Initialized alszam counter for foszam. IktatoszamId: {IktatoszamId}; IktatokonyvId: {IktatokonyvId}; Foszam: {Foszam}",
                iktatoszam.Id,
                iktatoszam.IktatokonyvId,
                iktatoszam.Foszam);
        }
    }
}
