using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("marketstat");
        
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

        modelBuilder.Entity<DimJobDbModel>(b =>
        {
            b.ToTable("dim_job");
            b.HasKey(j => j.JobId);
            b.Property(j => j.JobId).HasColumnName("job_id").UseIdentityByDefaultColumn();
            b.Property(j => j.JobRoleTitle).HasColumnName("job_role_title").HasMaxLength(255).IsRequired();
            b.Property(j => j.StandardJobRoleTitle).HasColumnName("standard_job_role_title").HasMaxLength(255).IsRequired();
            b.Property(j => j.HierarchyLevelName).HasColumnName("hierarchy_level_name").HasMaxLength(255).IsRequired();
            b.Property(j => j.IndustryFieldId).HasColumnName("industry_field_id").IsRequired();

            b.HasIndex(j => new { j.JobRoleTitle, j.StandardJobRoleTitle, j.HierarchyLevelName, j.IndustryFieldId })
                .IsUnique()
                .HasDatabaseName("uq_dim_job");
            
            b.HasOne(j => j.IndustryField)
                .WithMany(i => i.DimJobs)
                .HasForeignKey(j => j.IndustryFieldId)
                .HasConstraintName("fk_dim_job_industry")
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
            b.ToTable("dim_employee", tb =>
            {
                tb.HasCheckConstraint("ck_career_after_birth", "\"career_start_date\" > \"birth_date\"");
                tb.HasCheckConstraint("ck_career_min_age",
                    "\"career_start_date\" >= \"birth_date\" + INTERVAL '16 years'");
                tb.HasCheckConstraint("ck_dim_emp_birth_date", "\"birth_date\" <= CURRENT_DATE");
                tb.HasCheckConstraint("ck_dim_emp_career_start", "\"career_start_date\" <= CURRENT_DATE");
            });
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
            
            b.Property(e => e.GraduationYear)
                .HasColumnName("graduation_year")
                .IsRequired(false);

            b.Property(e => e.EducationId)
                .HasColumnName("education_id")
                .IsRequired(false);
            
            b.HasOne(e => e.Education)
                .WithMany(edu => edu.DimEmployees)
                .HasForeignKey(e => e.EducationId)
                .HasConstraintName("fk_dim_employee_education")
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DimLocationDbModel>(b =>
        {
            b.ToTable("dim_location");
            b.HasKey(l => l.LocationId);
            b.Property(l => l.LocationId).HasColumnName("location_id").UseIdentityByDefaultColumn();
            b.Property(l => l.CityName).HasColumnName("city_name").HasMaxLength(255).IsRequired();
            b.Property(l => l.OblastName).HasColumnName("oblast_name").HasMaxLength(255).IsRequired();
            b.Property(l => l.DistrictName).HasColumnName("district_name").HasMaxLength(255).IsRequired();
            b.HasIndex(l => new { l.CityName, l.OblastName, l.DistrictName })
                .IsUnique()
                .HasDatabaseName("uq_dim_location");
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
            b.Property(x => x.LocationId)
                .HasColumnName("location_id")
                .IsRequired();
            b.Property(x => x.EmployerId)
                .HasColumnName("employer_id")
                .IsRequired();
            b.Property(x => x.JobId)
                .HasColumnName("job_id")
                .IsRequired();
            b.Property(x => x.EmployeeId)
                .HasColumnName("employee_id")
                .IsRequired();

            b.Property(x => x.SalaryAmount)
                .HasColumnName("salary_amount")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            b.HasOne(fs => fs.DimDate)
                .WithMany(d => d.FactSalaries)
                .HasForeignKey(fs => fs.DateId)
                .HasConstraintName("fk_fact_date")
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(fs => fs.DimLocation)
                .WithMany(l => l.FactSalaries)
                .HasForeignKey(fs => fs.LocationId)
                .HasConstraintName("fk_fact_location")
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(fs => fs.DimEmployer)
                .WithMany(e => e.FactSalaries)
                .HasForeignKey(fs => fs.EmployerId)
                .HasConstraintName("fk_fact_employer")
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(fs => fs.DimJob)
                .WithMany(j => j.FactSalaries)
                .HasForeignKey(fs => fs.JobId)
                .HasConstraintName("fk_fact_job")
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(fs => fs.DimEmployee)
                .WithMany(e => e.FactSalaries)
                .HasForeignKey(fs => fs.EmployeeId)
                .HasConstraintName("fk_fact_employee")
                .OnDelete(DeleteBehavior.Restrict);
            
            b.HasIndex(fs => fs.DateId).HasDatabaseName("idx_fact_date");
            b.HasIndex(fs => fs.LocationId).HasDatabaseName("idx_fact_location");
            b.HasIndex(fs => fs.EmployerId).HasDatabaseName("idx_fact_employer");
            b.HasIndex(fs => fs.JobId).HasDatabaseName("idx_fact_job");
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

            b.Property(u => u.IsAdmin)
                .HasColumnName("is_admin")
                .IsRequired()
                .HasDefaultValue(false);
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
        
        modelBuilder.Entity<SalarySummaryDto>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable((string?)null);
        });
    }
}