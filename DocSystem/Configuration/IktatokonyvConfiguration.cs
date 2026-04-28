using DocSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocSystem.Configuration;

public class IktatokonyvConfiguration : EntityConfigurationBase<Iktatokonyv>
{
    public override void Configure(EntityTypeBuilder<Iktatokonyv> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Nev)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Kod)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Evszam)
            .IsRequired();

        builder.HasIndex(x => new { x.Kod, x.Evszam })
            .IsUnique();

        builder.HasMany(x => x.Iktatoszamok)
            .WithOne(x => x.Iktatokonyv)
            .HasForeignKey(x => x.IktatokonyvId)
            .IsRequired();

        builder.HasOne(x => x.FoszamCounter)
            .WithOne(x => x.Iktatokonyv)
            .HasForeignKey<FoszamCounter>(x => x.IktatokonyvId)
            .IsRequired();
    }
}
