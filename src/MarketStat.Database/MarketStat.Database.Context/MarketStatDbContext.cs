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
    public DbSet<DimIndustryFieldDbModel> DimIndustryFields { get; set; }
    public DbSet<DimJobRoleDbModel> DimJobRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimEmployerDbModel>().ToTable("dim_employers").HasKey(e => e.EmployerId);
        modelBuilder.Entity<DimIndustryFieldDbModel>().ToTable("dim_industry_fields").HasKey(i => i.IndustryFieldId);
        modelBuilder.Entity<DimJobRoleDbModel>().ToTable("dim_job_roles").HasKey(j => j.JobRoleId);
        base.OnModelCreating(modelBuilder);
    }
}