using Microsoft.EntityFrameworkCore;
using OceanicApi.Models;

namespace OceanicApi.Data;

public sealed class OceanicContext(DbContextOptions<OceanicContext> options) : DbContext(options)
{
    public DbSet<Berles> Berlesek => Set<Berles>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Berles>(entity =>
        {
            entity.ToTable("Berlesek");
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => new { b.YachtId, b.StartDate, b.EndDate });
            entity.Ignore(b => b.TotalPrice);
        });
    }
}
