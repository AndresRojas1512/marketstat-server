using MarketStat.Database.Models;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;
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
    public DbSet<DimHierarchyLevelDbModel> DimHierarchyLevels { get; set; }
    public DbSet<DimEmployerIndustryFieldDbModel> DimEmployerIndustryFields { get; set; }
    public DbSet<DimFederalDistrictDbModel> DimFederalDistricts { get; set; }
    public DbSet<DimOblastDbModel> DimOblasts { get; set; }
    public DbSet<DimCityDbModel> DimCities { get; set; }
    public DbSet<DimEducationLevelDbModel> DimEducationLevels { get; set; }
    public DbSet<DimStandardJobRoleHierarchyDbModel> DimStandardJobRoleHierarchies { get; set; }
    public DbSet<DimStandardJobRoleDbModel> DimStandardJobRoles { get; set; }
    
    public DbSet<FactSalaryDbModel> FactSalaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimEmployerDbModel>(b =>
        {
            b.ToTable("dim_employers");
            b.HasKey(e => e.EmployerId);
            b.Property(e => e.EmployerId)
                .HasColumnName("employer_id")
                .UseIdentityByDefaultColumn();
            b.Property(e => e.EmployerName)
                .HasColumnName("employer_name")
                .HasMaxLength(255)
                .IsRequired();
            b.Property(e => e.IsPublic)
                .HasColumnName("is_public")
                .IsRequired();
        });

        modelBuilder.Entity<DimIndustryFieldDbModel>(b =>
        {
            b.ToTable("dim_industry_fields");
            b.HasKey(i => i.IndustryFieldId);
            b.Property(i => i.IndustryFieldId)
                .HasColumnName("industry_field_id")
                .UseIdentityByDefaultColumn();
            b.Property(i => i.IndustryFieldName)
                .HasColumnName("industry_field_name")
                .HasMaxLength(255)
                .IsRequired();
        });

        modelBuilder.Entity<DimJobRoleDbModel>(b =>
        {
            b.ToTable("dim_job_roles");
            b.HasKey(j => j.JobRoleId);
            b.Property(j => j.JobRoleId)
                .HasColumnName("job_role_id")
                .UseIdentityByDefaultColumn();
            b.Property(j => j.JobRoleTitle)
                .HasColumnName("job_role_title")
                .HasMaxLength(255)
                .IsRequired();
            b.Property(j => j.StandardJobRoleId)
                .HasColumnName("standard_job_role_id")
                .IsRequired();
            b.Property(j => j.HierarchyLevelId)
                .HasColumnName("hierarchy_level_id")
                .IsRequired();
            b.HasOne<DimStandardJobRoleDbModel>()
                .WithMany()
                .HasForeignKey(j => j.StandardJobRoleId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<DimHierarchyLevelDbModel>()
                .WithMany()
                .HasForeignKey(j => j.HierarchyLevelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimDateDbModel>(b =>
        {
            b.ToTable("dim_date", tb =>
            {
                tb.HasCheckConstraint(
                    "CK_dim_date_quarter",
                    "\"quarter\" BETWEEN 1 AND 4");
                tb.HasCheckConstraint(
                    "CK_dim_date_month",
                    "\"month\" BETWEEN 1 AND 12");
            });
            b.HasKey(d => d.DateId);
            b.Property(d => d.DateId)
                .HasColumnName("date_id")
                .UseIdentityByDefaultColumn();
            b.Property(d => d.FullDate)
                .HasColumnName("full_date")
                .HasColumnType("date")
                .IsRequired();
            b.Property(d => d.Year)
                .HasColumnName("year")
                .IsRequired();
            b.Property(d => d.Quarter)
                .HasColumnName("quarter")
                .IsRequired();
            b.Property(d => d.Month)
                .HasColumnName("month")
                .IsRequired();
        });

        modelBuilder.Entity<DimEducationDbModel>(b =>
        {
            b.ToTable("dim_education");
            b.HasKey(e => e.EducationId);
            b.Property(e => e.EducationId)
                .HasColumnName("education_id")
                .UseIdentityByDefaultColumn();
            b.Property(e => e.Specialty)
                .HasColumnName("specialty")
                .HasMaxLength(255)
                .IsRequired();
            b.Property(e => e.SpecialtyCode)
                .HasColumnName("specialty_code")
                .HasMaxLength(255)
                .IsRequired();
            b.Property(e => e.EducationLevelId)
                .HasColumnName("education_level_id")
                .IsRequired();
            b.HasOne<DimEducationLevelDbModel>()
                .WithMany()
                .HasForeignKey(e => e.EducationLevelId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(e => e.IndustryFieldId)
                .HasColumnName("industry_field_id")
                .IsRequired();
            b.HasOne<DimIndustryFieldDbModel>()
                .WithMany()
                .HasForeignKey(e => e.IndustryFieldId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimEmployeeDbModel>(b =>
        {
            b.ToTable("dim_employees");
            b.HasKey(e => e.EmployeeId);
            b.Property(e => e.EmployeeId)
                .HasColumnName("employee_id")
                .UseIdentityByDefaultColumn();
            b.Property(e => e.BirthDate)
                .HasColumnName("birth_date")
                .HasColumnType("date")
                .IsRequired();
            b.Property(e => e.CareerStartDate)
                .HasColumnName("career_start_date")
                .HasColumnType("date")
                .IsRequired();
        });

        modelBuilder.Entity<DimEmployeeEducationDbModel>(b =>
        {
            b.ToTable("dim_employee_education");
            b.HasKey(x => new { x.EmployeeId, x.EducationId });
            b.Property(x => x.EmployeeId)
                .HasColumnName("employee_id")
                .ValueGeneratedNever();
            b.Property(x => x.EducationId)
                .HasColumnName("education_id")
                .ValueGeneratedNever();
            b.Property(x => x.GraduationYear)
                .HasColumnName("graduation_year")
                .IsRequired();

            b.HasOne<DimEmployeeDbModel>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<DimEducationDbModel>()
                .WithMany()
                .HasForeignKey(x => x.EducationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimHierarchyLevelDbModel>(b =>
        {
            b.ToTable("dim_hierarchy_levels");
            b.HasKey(h => h.HierarchyLevelId);
            b.Property(h => h.HierarchyLevelId)
                .HasColumnName("hierarchy_level_id")
                .UseIdentityByDefaultColumn();
            b.Property(h => h.HierarchyLevelName)
                .HasColumnName("hierarchy_level_name")
                .HasMaxLength(255)
                .IsRequired();
        });

        modelBuilder.Entity<DimEmployerIndustryFieldDbModel>(b =>
        {
            b.ToTable("dim_employer_industry_field");
            b.HasKey(x => new { x.EmployerId, x.IndustryFieldId });
            b.Property(x => x.EmployerId)
                .HasColumnName("employer_id")
                .ValueGeneratedNever();
            b.Property(x => x.IndustryFieldId)
                .HasColumnName("industry_field_id")
                .ValueGeneratedNever();

            b.HasOne<DimEmployerDbModel>()
                .WithMany()
                .HasForeignKey(x => x.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<DimIndustryFieldDbModel>()
                .WithMany()
                .HasForeignKey(x => x.IndustryFieldId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimFederalDistrictDbModel>(b =>
        {
            b.ToTable("dim_federal_districts");
            b.HasKey(f => f.DistrictId);
            b.Property(f => f.DistrictId)
                .HasColumnName("district_id")
                .UseIdentityByDefaultColumn();
            b.Property(f => f.DistrictName)
                .HasColumnName("district_name")
                .HasMaxLength(255)
                .IsRequired();
        });

        modelBuilder.Entity<DimOblastDbModel>(b =>
        {
            b.ToTable("dim_oblasts");
            b.HasKey(o => o.OblastId);
            b.Property(o => o.OblastId)
                .HasColumnName("oblast_id")
                .UseIdentityByDefaultColumn();
            b.Property(o => o.OblastName)
                .HasColumnName("oblast_name")
                .HasMaxLength(255)
                .IsRequired();
            b.Property(o => o.DistrictId)
                .HasColumnName("district_id")
                .IsRequired();

            b.HasOne<DimFederalDistrictDbModel>()
                .WithMany()
                .HasForeignKey(o => o.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimCityDbModel>(b =>
        {
            b.ToTable("dim_cities");
            b.HasKey(c => c.CityId);
            b.Property(c => c.CityId)
                .HasColumnName("city_id")
                .UseIdentityByDefaultColumn();
            b.Property(c => c.CityName)
                .HasColumnName("city_name")
                .HasMaxLength(255)
                .IsRequired();
            b.Property(c => c.OblastId)
                .HasColumnName("oblast_id")
                .IsRequired();
            b.HasOne<DimOblastDbModel>()
                .WithMany()
                .HasForeignKey(c => c.OblastId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimEducationLevelDbModel>(b =>
        {
            b.ToTable("dim_education_levels");
            b.HasKey(e => e.EducationLevelId);
            b.Property(e => e.EducationLevelId)
                .HasColumnName("education_level_id")
                .UseIdentityByDefaultColumn();
            b.Property(e => e.EducationLevelName)
                .HasColumnName("education_level_name")
                .HasMaxLength(255)
                .IsRequired();
        });

        modelBuilder.Entity<DimStandardJobRoleDbModel>(b =>
        {
            b.ToTable("dim_standard_job_roles");
            b.HasKey(j => j.StandardJobRoleId);
            b.Property(j => j.StandardJobRoleId)
                .HasColumnName("standard_job_role_id")
                .UseIdentityByDefaultColumn();
            b.Property(j => j.StandardJobRoleTitle)
                .HasColumnName("standard_job_role_title")
                .HasMaxLength(255)
                .IsRequired();
            b.Property(j => j.IndustryFieldId)
                .HasColumnName("industry_field_id")
                .IsRequired();
            b.HasOne<DimIndustryFieldDbModel>()
                .WithMany()
                .HasForeignKey(x => x.IndustryFieldId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimStandardJobRoleHierarchyDbModel>(b =>
        {
            b.ToTable("dim_standard_job_role_hierarchy");
            b.HasKey(x => new { x.StandardJobRoleId, x.HierarchyLevelId });
            b.Property(x => x.StandardJobRoleId)
                .HasColumnName("standard_job_role_id")
                .ValueGeneratedNever();
            b.Property(x => x.HierarchyLevelId)
                .HasColumnName("hierarchy_level_id")
                .ValueGeneratedNever();
            b.HasOne<DimStandardJobRoleDbModel>()
                .WithMany()
                .HasForeignKey(x => x.StandardJobRoleId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<DimHierarchyLevelDbModel>()
                .WithMany()
                .HasForeignKey(x => x.HierarchyLevelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FactSalaryDbModel>(b =>
        {
            b.ToTable("fact_salaries");
            b.HasKey(s => s.SalaryFactId);
            b.Property(s => s.SalaryFactId)
                .HasColumnName("salary_fact_id")
                .UseIdentityByDefaultColumn();
            b.Property(x => x.DateId)
                .HasColumnName("date_id")
                .IsRequired();
            b.Property(x => x.CityId)
                .HasColumnName("city_id")
                .IsRequired();
            b.Property(x => x.EmployerId)
                .HasColumnName("employer_id")
                .IsRequired();
            b.Property(x => x.JobRoleId)
                .HasColumnName("job_role_id")
                .IsRequired();
            b.Property(x => x.EmployeeId)
                .HasColumnName("employee_id")
                .IsRequired();
            b.Property(x => x.SalaryAmount)
                .HasColumnName("salary_amount")
                .HasColumnType("numeric(18,2)")
                .IsRequired();
            b.Property(x => x.BonusAmount)
                .HasColumnName("bonus_amount")
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0m)
                .IsRequired();
            b.HasOne<DimDateDbModel>()
                .WithMany()
                .HasForeignKey(x => x.DateId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<DimCityDbModel>()
                .WithMany()
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<DimEmployerDbModel>()
                .WithMany()
                .HasForeignKey(x => x.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<DimJobRoleDbModel>()
                .WithMany()
                .HasForeignKey(x => x.JobRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<DimEmployeeDbModel>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    base.OnModelCreating(modelBuilder);
    }
}