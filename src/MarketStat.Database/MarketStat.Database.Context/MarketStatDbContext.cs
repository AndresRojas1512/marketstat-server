using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Context;

public class MarketStatDbContext : DbContext
{
    public MarketStatDbContext(DbContextOptions<MarketStatDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<DimEmployerDbModel> DimEmployers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimEmployerDbModel>().ToTable("dim_employers").HasKey(e => e.EmployerId);
        base.OnModelCreating(modelBuilder);
    }
}