using DocSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocSystem.Configuration;

public class AlszamCounterConfiguration : EntityConfigurationBase<AlszamCounter>
{
    public override void Configure(EntityTypeBuilder<AlszamCounter> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Next)
            .IsRequired();

        builder.HasIndex(x => x.IktatoszamId)
            .IsUnique();
    }
}
