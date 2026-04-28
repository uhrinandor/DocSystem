using DocSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DocSystem.Configuration;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Iktatokonyv> Iktatokonyvek => Set<Iktatokonyv>();
    public DbSet<Iktatoszam> Iktatoszamok => Set<Iktatoszam>();
    public DbSet<FoszamCounter> FoszamCounters => Set<FoszamCounter>();
    public DbSet<AlszamCounter> AlszamCounters => Set<AlszamCounter>();
    public DbSet<Irat> Iratok => Set<Irat>();
    public DbSet<Ugyirat> Ugyiratok => Set<Ugyirat>();
    public DbSet<User> Users => Set<User>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyAuditing();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditing();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditing()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.Id == Guid.Empty)
                    entry.Entity.Id = Guid.NewGuid();
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
