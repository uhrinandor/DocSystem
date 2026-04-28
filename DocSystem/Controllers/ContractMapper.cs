using DocSystem.Models;
using DocSystem.Services;
using IktatokonyvContract = DocSystem.TypeContracts.Iktatokonyv;
using IktatokonyvReferenceContract = DocSystem.TypeContracts.IktatokonyvReference;
using IktatoszamReferenceContract = DocSystem.TypeContracts.IktatoszamReference;
using IratContract = DocSystem.TypeContracts.Irat;
using IratReferenceContract = DocSystem.TypeContracts.IratReference;
using UgyiratContract = DocSystem.TypeContracts.Ugyirat;
using UgyiratReferenceContract = DocSystem.TypeContracts.UgyiratReference;
using UserContract = DocSystem.TypeContracts.User;

namespace DocSystem.Controllers;

public class ContractMapper(IktatoszamFormatter iktatoszamFormatter)
{
    public IktatokonyvContract MapIktatokonyv(Iktatokonyv iktatokonyv)
    {
        return new IktatokonyvContract
        {
            Id = iktatokonyv.Id,
            CreatedAt = iktatokonyv.CreatedAt,
            UpdatedAt = iktatokonyv.UpdatedAt,
            Nev = iktatokonyv.Nev,
            Kod = iktatokonyv.Kod,
            Evszam = iktatokonyv.Evszam,
            Iktatoszamok = iktatokonyv.Iktatoszamok
                .Select(MapIktatoszamReference)
                .ToList()
        };
    }

    public IktatokonyvReferenceContract MapIktatokonyvReference(Iktatokonyv iktatokonyv)
    {
        return new IktatokonyvReferenceContract
        {
            Id = iktatokonyv.Id,
            CreatedAt = iktatokonyv.CreatedAt,
            UpdatedAt = iktatokonyv.UpdatedAt,
            Nev = iktatokonyv.Nev,
            Kod = iktatokonyv.Kod,
            Evszam = iktatokonyv.Evszam
        };
    }

    public IktatoszamReferenceContract MapIktatoszamReference(Iktatoszam iktatoszam)
    {
        return new IktatoszamReferenceContract
        {
            Id = iktatoszam.Id,
            CreatedAt = iktatoszam.CreatedAt,
            UpdatedAt = iktatoszam.UpdatedAt,
            Foszam = iktatoszam.Foszam,
            Alszam = iktatoszam.Alszam,
            Valid = iktatoszam.Valid,
            SzovegesIktatoszam = iktatoszamFormatter.Format(iktatoszam),
            Iktatokonyv = MapIktatokonyvReference(iktatoszam.Iktatokonyv)
        };
    }

    public IratContract MapIrat(Irat irat)
    {
        return new IratContract
        {
            Id = irat.Id,
            CreatedAt = irat.CreatedAt,
            UpdatedAt = irat.UpdatedAt,
            Subject = irat.Subject,
            Details = irat.Details,
            Iktatoszam = MapIktatoszamReference(irat.Iktatoszam),
            Ugyirat = MapUgyiratReference(irat.Ugyirat)
        };
    }

    public IratReferenceContract MapIratReference(Irat irat)
    {
        return new IratReferenceContract
        {
            Id = irat.Id,
            CreatedAt = irat.CreatedAt,
            UpdatedAt = irat.UpdatedAt,
            Subject = irat.Subject,
            Details = irat.Details
        };
    }

    public UgyiratContract MapUgyirat(Ugyirat ugyirat)
    {
        return new UgyiratContract
        {
            Id = ugyirat.Id,
            CreatedAt = ugyirat.CreatedAt,
            UpdatedAt = ugyirat.UpdatedAt,
            Iktatoszam = MapIktatoszamReference(ugyirat.Iktatoszam),
            Irats = ugyirat.Irats
                .Select(MapIratReference)
                .ToList()
        };
    }

    public UgyiratReferenceContract MapUgyiratReference(Ugyirat ugyirat)
    {
        return new UgyiratReferenceContract
        {
            Id = ugyirat.Id,
            CreatedAt = ugyirat.CreatedAt,
            UpdatedAt = ugyirat.UpdatedAt
        };
    }

    public UserContract MapUser(User user)
    {
        return new UserContract
        {
            Id = user.Id,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Name = user.Name,
            UserName = user.UserName
        };
    }
}
