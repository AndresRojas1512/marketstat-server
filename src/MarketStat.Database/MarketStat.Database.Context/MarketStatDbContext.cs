using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Database.Models;
using MarketStat.Database.Models.MarketStat.Database.Models.Account;
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
    public DbSet<DimJobDbModel> DimJobs { get; set; }
    public DbSet<DimDateDbModel> DimDates { get; set; }
    public DbSet<DimEducationDbModel> DimEducations { get; set; }
    public DbSet<DimEmployeeDbModel> DimEmployees { get; set; }
    public DbSet<DimLocationDbModel> DimLocations { get; set; }
    
    public DbSet<FactSalaryDbModel> FactSalaries { get; set; }
    
    public DbSet<UserDbModel> Users { get; set; }
    public DbSet<BenchmarkHistoryDbModel> BenchmarkHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("marketstat");
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<DimEmployerDbModel>(b =>
        {
            b.ToTable("dim_employer");
            b.HasKey(e => e.EmployerId);
            b.Property(e => e.EmployerId)
                .HasColumnName("employer_id")
                .UseIdentityByDefaultColumn();

            b.Property(e => e.EmployerName)
                .HasColumnName("employer_name")
                .HasMaxLength(255)
                .IsRequired();
            b.HasIndex(e => e.EmployerName)
                .IsUnique()
                .HasDatabaseName("uq_dim_employer_name");
            
            b.Property(e => e.Inn)
                .HasColumnName("inn")
                .HasMaxLength(12)
                .IsRequired();
            b.HasIndex(e => e.Inn)
                .IsUnique()
                .HasDatabaseName("uq_dim_employer_inn");

            b.Property(e => e.Ogrn)
                .HasColumnName("ogrn")
                .HasMaxLength(13)
                .IsRequired();
            b.HasIndex(e => e.Ogrn)
                .IsUnique()
                .HasDatabaseName("uq_dim_employer_ogrn");

            b.Property(e => e.Kpp)
                .HasColumnName("kpp")
                .HasMaxLength(9)
                .IsRequired();

            b.Property(e => e.RegistrationDate)
                .HasColumnName("registration_date")
                .HasColumnType("date")
                .IsRequired();

            b.Property(e => e.LegalAddress)
                .HasColumnName("legal_address")
                .HasColumnType("text")
                .IsRequired();

            b.Property(e => e.ContactEmail)
                .HasColumnName("contact_email")
                .HasMaxLength(255)
                .IsRequired();

            b.Property(e => e.ContactPhone)
                .HasColumnName("contact_phone")
                .HasMaxLength(50)
                .IsRequired();

            b.Property(e => e.IndustryFieldId)
                .HasColumnName("industry_field_id")
                .IsRequired();
            
            b.HasOne(e => e.DimIndustryField)
                .WithMany(i => i.DimEmployers)
                .HasForeignKey(e => e.IndustryFieldId)
                .HasConstraintName("fk_dim_employer_industry")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimIndustryFieldDbModel>(b =>
        {
            b.ToTable("dim_industry_field");
            b.HasKey(i => i.IndustryFieldId);
        
            b.Property(i => i.IndustryFieldId)
                .HasColumnName("industry_field_id")
                .UseIdentityByDefaultColumn();
        
            b.Property(i => i.IndustryFieldCode)
                .HasColumnName("industry_field_code")
                .HasMaxLength(10)
                .IsRequired();
            b.HasIndex(i => i.IndustryFieldCode)
                .IsUnique()
                .HasDatabaseName("uq_dim_industry_field_code");
        
            b.Property(i => i.IndustryFieldName)
                .HasColumnName("industry_field_name")
                .HasMaxLength(255)
                .IsRequired();
            b.HasIndex(i => i.IndustryFieldName)
                .IsUnique()
                .HasDatabaseName("uq_dim_industry_field_name");
        });

        modelBuilder.Entity<DimJobRoleDbModel>(b =>
        {
            b.ToTable("dim_job_role");
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
            b.HasIndex(j => new { j.JobRoleTitle, j.StandardJobRoleId, j.HierarchyLevelId })
                .IsUnique()
                .HasDatabaseName("uq_dim_job_role_natural_key");
            b.HasOne(jr => jr.DimStandardJobRole)
                .WithMany(sjr => sjr.DimJobRoles)
                .HasForeignKey(jr => jr.StandardJobRoleId)
                .HasConstraintName("fk_dim_jr_sjr")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(jr => jr.DimHierarchyLevel)
                .WithMany(hl => hl.DimJobRoles)
                .HasForeignKey(jr => jr.HierarchyLevelId)
                .HasConstraintName("fk_dim_jr_hl")
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
            b.HasIndex(d => d.FullDate).IsUnique();
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
            b.Property(e => e.SpecialtyName)
                .HasColumnName("specialty_name")
                .HasMaxLength(255)
                .IsRequired();
            b.Property(e => e.SpecialtyCode)
                .HasColumnName("specialty_code")
                .HasMaxLength(255)
                .IsRequired();
            b.Property(e => e.EducationLevelName)
                .HasColumnName("education_level_name")
                .HasMaxLength(255)
                .IsRequired();
            b.HasIndex(e => new { e.SpecialtyName, e.EducationLevelName })
                .IsUnique()
                .HasDatabaseName("uq_dim_education");
        });

        modelBuilder.Entity<DimEmployeeDbModel>(b =>
        {
            b.ToTable("dim_employee");
            b.HasKey(e => e.EmployeeId);

            b.Property(e => e.EmployeeId)
                .HasColumnName("employee_id")
                .UseIdentityByDefaultColumn();

            b.Property(e => e.EmployeeRefId)
                .HasColumnName("employee_ref_id")
                .HasMaxLength(255)
                .IsRequired();
            b.HasIndex(e => e.EmployeeRefId)
                .IsUnique()
                .HasDatabaseName("uq_dim_employee_ref_id");

            b.Property(e => e.BirthDate)
                .HasColumnName("birth_date")
                .HasColumnType("date")
                .IsRequired();
            b.Property(e => e.CareerStartDate)
                .HasColumnName("career_start_date")
                .HasColumnType("date")
                .IsRequired();

            b.Property(e => e.Gender)
                .HasColumnName("gender")
                .HasMaxLength(50)
                .IsRequired(false);
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

            b.HasOne(fs => fs.DimDate)
                .WithMany(d => d.FactSalaries)
                .HasForeignKey(fs => fs.DateId)
                .HasConstraintName("fk_fact_date")
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(fs => fs.DimCity)
                .WithMany(c => c.FactSalaries)
                .HasForeignKey(fs => fs.CityId)
                .HasConstraintName("fk_fact_city")
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(fs => fs.DimEmployer)
                .WithMany(e => e.FactSalaries)
                .HasForeignKey(fs => fs.EmployerId)
                .HasConstraintName("fk_fact_emp")
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(fs => fs.DimJobRole)
                .WithMany(jr => jr.FactSalaries)
                .HasForeignKey(fs => fs.JobRoleId)
                .HasConstraintName("fk_fact_jrole")
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(fs => fs.DimEmployee)
                .WithMany(emp => emp.FactSalaries)
                .HasForeignKey(fs => fs.EmployeeId)
                .HasConstraintName("fk_fact_employee")
                .OnDelete(DeleteBehavior.Restrict);
            
            b.HasIndex(fs => fs.DateId).HasDatabaseName("idx_fact_date");
            b.HasIndex(fs => fs.CityId).HasDatabaseName("idx_fact_city");
            b.HasIndex(fs => fs.EmployerId).HasDatabaseName("idx_fact_employer");
            b.HasIndex(fs => fs.JobRoleId).HasDatabaseName("idx_fact_jrole");
            b.HasIndex(fs => fs.EmployeeId).HasDatabaseName("idx_fact_employee");
        });

        modelBuilder.Entity<UserDbModel>(b =>
        {
            b.ToTable("users"); 
            b.HasKey(u => u.UserId);
            b.Property(u => u.UserId)
                .HasColumnName("user_id")
                .UseIdentityByDefaultColumn();

            b.Property(u => u.Username)
                .HasColumnName("username")
                .HasMaxLength(100)
                .IsRequired();
            b.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("uq_users_username");

            b.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasColumnType("text")
                .IsRequired();

            b.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();
            b.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("uq_users_email");

            b.Property(u => u.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(255)
                .IsRequired();

            b.Property(u => u.IsActive)
                .HasColumnName("is_active")
                .IsRequired()
                .HasDefaultValue(true);

            b.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            b.Property(u => u.LastLoginAt)
                .HasColumnName("last_login_at")
                .IsRequired(false);

            b.Property(u => u.SavedBenchmarksCount)
                .HasColumnName("saved_benchmarks_count")
                .IsRequired()
                .HasDefaultValue(0);

            b.Property(u => u.IsEtlUser)
                .HasColumnName("is_etl_user")
                .IsRequired()
                .HasDefaultValue(false);

            b.HasMany(u => u.BenchmarkHistories)
                .WithOne(bh => bh.User) 
                .HasForeignKey(bh => bh.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<BenchmarkHistoryDbModel>(b =>
        {
            b.ToTable("benchmark_history");
            b.HasKey(bh => bh.BenchmarkHistoryId);
            b.Property(bh => bh.BenchmarkHistoryId)
                .HasColumnName("benchmark_history_id")
                .UseIdentityByDefaultColumn();
            b.Property(bh => bh.UserId)
                .HasColumnName("user_id")
                .IsRequired();
            b.Property(bh => bh.BenchmarkName)
                .HasColumnName("benchmark_name")
                .HasMaxLength(255)
                .IsRequired(false);
            b.Property(bh => bh.SavedAt)
                .HasColumnName("saved_at")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(bh => bh.FilterIndustryFieldId)
                .HasColumnName("filter_industry_field_id")
                .IsRequired(false);
            b.Property(bh => bh.FilterStandardJobRoleId)
                .HasColumnName("filter_standard_job_role_id")
                .IsRequired(false);
            b.Property(bh => bh.FilterHierarchyLevelId)
                .HasColumnName("filter_hierarchy_level_id")
                .IsRequired(false);
            b.Property(bh => bh.FilterDistrictId)
                .HasColumnName("filter_district_id")
                .IsRequired(false);
            b.Property(bh => bh.FilterOblastId)
                .HasColumnName("filter_oblast_id")
                .IsRequired(false);
            b.Property(bh => bh.FilterCityId)
                .HasColumnName("filter_city_id")
                .IsRequired(false);
            b.Property(bh => bh.FilterDateStart)
                .HasColumnName("filter_date_start")
                .HasColumnType("date")
                .IsRequired(false);
            b.Property(bh => bh.FilterDateEnd)
                .HasColumnName("filter_date_end")
                .HasColumnType("date")
                .IsRequired(false);
            b.Property(bh => bh.FilterTargetPercentile)
                .HasColumnName("filter_target_percentile")
                .IsRequired(false);
            b.Property(bh => bh.FilterGranularity)
                .HasColumnName("filter_granularity")
                .HasColumnType("text")
                .IsRequired(false); 
            b.Property(bh => bh.FilterPeriods)
                .HasColumnName("filter_periods")
                .IsRequired(false);
            b.Property(bh => bh.BenchmarkResultJson)
                .HasColumnName("benchmark_result_json")
                .HasColumnType("jsonb")
                .IsRequired();
            
            b.HasOne(bh => bh.User)
                .WithMany(u => u.BenchmarkHistories)
                .HasForeignKey(bh => bh.UserId)
                .HasConstraintName("fk_benchmark_history_user")
                .OnDelete(DeleteBehavior.Cascade);
            
            b.HasIndex(bh => bh.UserId).HasDatabaseName("idx_benchmark_history_user_id");
            b.HasIndex(bh => bh.SavedAt).HasDatabaseName("idx_benchmark_history_saved_at");
        });
        
        modelBuilder.Entity<FactSalary>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable((string?)null);
        });
        
        modelBuilder.Entity<SalaryDistributionBucketDto>(dto =>
        {
            dto.HasNoKey();
            dto.ToTable((string?)null);
        });

        modelBuilder.Entity<SalaryTimeSeriesPointDto>(dto =>
        {
            dto.HasNoKey();
            dto.ToTable((string?)null);
        });

        modelBuilder.Entity<PublicRoleByLocationIndustryDto>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable((string?)null);
        });

        modelBuilder.Entity<PublicSalaryByEducationInIndustryDto>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable((string?)null);
        });

        modelBuilder.Entity<PublicTopEmployerRoleSalariesInIndustryDto>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable((string?)null);
        });
        
        modelBuilder.Entity<SalarySummaryDto>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable((string?)null);
        });
    }
}