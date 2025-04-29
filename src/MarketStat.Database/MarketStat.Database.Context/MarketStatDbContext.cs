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
    public DbSet<DimDateDbModel> DimDates { get; set; }
    public DbSet<DimEducationDbModel> DimEducations { get; set; }
    public DbSet<DimEmployeeDbModel> DimEmployees { get; set; }
    public DbSet<DimEmployeeEducationDbModel> DimEmployeeEducations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimEmployerDbModel>()
            .ToTable("dim_employers")
            .HasKey(e => e.EmployerId);
        
        modelBuilder.Entity<DimIndustryFieldDbModel>()
            .ToTable("dim_industry_fields")
            .HasKey(i => i.IndustryFieldId);
        
        modelBuilder.Entity<DimJobRoleDbModel>()
            .ToTable("dim_job_roles")
            .HasKey(j => j.JobRoleId);
        
        modelBuilder.Entity<DimDateDbModel>()
            .ToTable("dim_date")
            .HasKey(d => d.DateId);
        
        modelBuilder.Entity<DimEducationDbModel>()
            .ToTable("dim_education")
            .HasKey(e => e.EducationId);
        
        modelBuilder.Entity<DimEmployeeDbModel>()
            .ToTable("dim_employees")
            .HasKey(e => e.EmployeeId);
        
        modelBuilder.Entity<DimEmployeeEducationDbModel>()
            .ToTable("dim_employee_education")
            .HasKey(e => new { e.EmployeeId, e.EducationId });
        
        base.OnModelCreating(modelBuilder);
    }
}