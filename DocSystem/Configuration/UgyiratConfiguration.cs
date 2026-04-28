using DocSystem.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocSystem.Configuration;

public class UgyiratConfiguration : EntityConfigurationBase<Ugyirat>
{
    public override void Configure(EntityTypeBuilder<Ugyirat> builder)
    {
        base.Configure(builder);

        builder.HasMany(x => x.Irats)
            .WithOne(x => x.Ugyirat)
            .HasForeignKey("UgyiratId")
            .IsRequired();

        builder.HasOne(x => x.Iktatoszam)
            .WithOne()
            .HasForeignKey<Ugyirat>("IktatoszamId")
            .IsRequired();
    }
}
