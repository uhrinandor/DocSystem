using DocSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocSystem.Configuration;

public class IktatoszamConfiguration : EntityConfigurationBase<Iktatoszam>
{
    public override void Configure(EntityTypeBuilder<Iktatoszam> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Foszam)
            .IsRequired();

        builder.Property(x => x.Alszam)
            .IsRequired();

        builder.Property(x => x.Valid)
            .IsRequired();

        builder.HasIndex(x => new { x.IktatokonyvId, x.Foszam, x.Alszam })
            .IsUnique();

        builder.HasOne(x => x.AlszamCounter)
            .WithOne(x => x.Iktatoszam)
            .HasForeignKey<AlszamCounter>(x => x.IktatoszamId);
    }
}
