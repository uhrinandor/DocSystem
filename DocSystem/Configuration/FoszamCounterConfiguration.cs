using DocSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocSystem.Configuration;

public class FoszamCounterConfiguration : EntityConfigurationBase<FoszamCounter>
{
    public override void Configure(EntityTypeBuilder<FoszamCounter> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Next)
            .IsRequired();

        builder.HasIndex(x => x.IktatokonyvId)
            .IsUnique();
    }
}
