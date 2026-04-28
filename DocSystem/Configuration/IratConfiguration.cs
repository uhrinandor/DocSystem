using DocSystem.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocSystem.Configuration;

public class IratConfiguration : EntityConfigurationBase<Irat>
{
    public override void Configure(EntityTypeBuilder<Irat> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Subject)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Details)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasOne(x => x.Iktatoszam)
            .WithMany()
            .HasForeignKey("IktatoszamId")
            .IsRequired();

        builder.HasOne(x => x.Ugyirat)
            .WithMany(x => x.Irats)
            .HasForeignKey("UgyiratId")
            .IsRequired();
    }
}
