namespace MarketStat.Database.Context;

using MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.Facts;
using MarketStat.Common.Dto.Facts.Analytics.Payloads;
using MarketStat.Database.Models;
using MarketStat.Database.Models.Account;
using MarketStat.Database.Models.Facts;
using Microsoft.EntityFrameworkCore;

public class MarketStatDbContext : DbContext
{
    public MarketStatDbContext(DbContextOptions<MarketStatDbContext> options)
        : base(options)
    {
    }

    public DbSet<DimEmployerDbModel> DimEmployers { get; set; } = null!;

    public DbSet<DimIndustryFieldDbModel> DimIndustryFields { get; set; } = null!;

    public DbSet<DimJobDbModel> DimJobs { get; set; } = null!;

    public DbSet<DimDateDbModel> DimDates { get; set; } = null!;

    public DbSet<DimEducationDbModel> DimEducations { get; set; } = null!;

    public DbSet<DimEmployeeDbModel> DimEmployees { get; set; } = null!;

    public DbSet<DimLocationDbModel> DimLocations { get; set; } = null!;

    public DbSet<FactSalaryDbModel> FactSalaries { get; set; } = null!;

    public DbSet<UserDbModel> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("marketstat");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketStatDbContext).Assembly);
    }
}
