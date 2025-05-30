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
            b.Property(e => e.IsPublic)
                .HasColumnName("is_public")
                .IsRequired()
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<DimIndustryFieldDbModel>(b =>
        {
            b.ToTable("dim_industry_field");
            b.HasKey(i => i.IndustryFieldId);
            b.Property(i => i.IndustryFieldId)
                .HasColumnName("industry_field_id")
                .UseIdentityByDefaultColumn();
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
            b.HasOne(edu => edu.DimEducationLevel)
                .WithMany(el => el.DimEducations)
                .HasForeignKey(edu => edu.EducationLevelId)
                .HasConstraintName("fk_dim_edu_lvl")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimEmployeeDbModel>(b =>
        {
            b.ToTable("dim_employee");
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
            b.HasIndex(e => new { e.BirthDate, e.CareerStartDate })
                .IsUnique()
                .HasDatabaseName("uq_dim_employee_natural_key");
        });

        modelBuilder.Entity<DimEmployeeEducationDbModel>(b =>
        {
            b.ToTable("dim_employee_education");
            b.HasKey(ee => new { ee.EmployeeId, ee.EducationId })
                .HasName("pk_dim_ee");
            b.Property(ee => ee.EmployeeId)
                .HasColumnName("employee_id")
                .ValueGeneratedNever(); 
            b.Property(ee => ee.EducationId)
                .HasColumnName("education_id")
                .ValueGeneratedNever(); 
            b.Property(ee => ee.GraduationYear)
                .HasColumnName("graduation_year")
                .IsRequired();
            b.HasOne(ee => ee.Employee)
                .WithMany(e => e.DimEmployeeEducations)
                .HasForeignKey(ee => ee.EmployeeId)
                .HasConstraintName("fk_dim_ee_emp")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(ee => ee.Education)
                .WithMany(edu => edu.DimEmployeeEducations)
                .HasForeignKey(ee => ee.EducationId)
                .HasConstraintName("fk_dim_ee_edu")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimHierarchyLevelDbModel>(b =>
        {
            b.ToTable("dim_hierarchy_level");
            b.HasKey(h => h.HierarchyLevelId);
            b.Property(h => h.HierarchyLevelId)
                .HasColumnName("hierarchy_level_id")
                .UseIdentityByDefaultColumn();
            b.Property(h => h.HierarchyLevelName)
                .HasColumnName("hierarchy_level_name")
                .HasMaxLength(255)
                .IsRequired();
            b.HasIndex(h => h.HierarchyLevelName)
                .IsUnique()
                .HasDatabaseName("uq_dim_hierarchy_level");
        });

        modelBuilder.Entity<DimEmployerIndustryFieldDbModel>(b =>
        {
            b.ToTable("dim_employer_industry_field");
            b.HasKey(eif => new { eif.EmployerId, eif.IndustryFieldId });
            b.Property(eif => eif.EmployerId)
                .HasColumnName("employer_id")
                .ValueGeneratedNever();
            b.Property(eif => eif.IndustryFieldId)
                .HasColumnName("industry_field_id")
                .ValueGeneratedNever();
            b.HasOne(eif => eif.Employer)
                .WithMany(e => e.EmployerIndustryFields)
                .HasForeignKey(eif => eif.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(eif => eif.IndustryField)
                .WithMany(i => i.EmployerIndustryFields)
                .HasForeignKey(eif => eif.IndustryFieldId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimFederalDistrictDbModel>(b =>
        {
            b.ToTable("dim_federal_district");
            b.HasKey(f => f.DistrictId);
            b.Property(f => f.DistrictId)
                .HasColumnName("district_id")
                .UseIdentityByDefaultColumn();
            b.Property(f => f.DistrictName)
                .HasColumnName("district_name")
                .HasMaxLength(255)
                .IsRequired();
            b.HasIndex(f => f.DistrictName).IsUnique();
        });

        modelBuilder.Entity<DimOblastDbModel>(b =>
        {
            b.ToTable("dim_oblast");
            b.HasKey(o => o.OblastId);
            b.Property(o => o.OblastId)
                .HasColumnName("oblast_id")
                .UseIdentityByDefaultColumn();
            b.Property(o => o.OblastName)
                .HasColumnName("oblast_name")
                .HasMaxLength(255)
                .IsRequired();
            b.HasIndex(o => o.OblastName).IsUnique();
            b.Property(o => o.DistrictId)
                .HasColumnName("district_id")
                .IsRequired();
            b.HasOne(o => o.DimFederalDistrict)
                .WithMany(federalDistrict => federalDistrict.DimOblasts)
                .HasForeignKey(o => o.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimCityDbModel>(b =>
        {
            b.ToTable("dim_city");
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
            b.HasIndex(c => new { c.CityName, c.OblastId })
                .IsUnique()
                .HasDatabaseName("uq_dim_city_oblast");
            b.HasOne(city => city.DimOblast)
                .WithMany(oblast => oblast.DimCities)
                .HasForeignKey(c => c.OblastId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimEducationLevelDbModel>(b =>
        {
            b.ToTable("dim_education_level");
            b.HasKey(e => e.EducationLevelId);
            b.Property(e => e.EducationLevelId)
                .HasColumnName("education_level_id")
                .UseIdentityByDefaultColumn();
            b.Property(e => e.EducationLevelName)
                .HasColumnName("education_level_name")
                .HasMaxLength(255)
                .IsRequired();
            b.HasIndex(e => e.EducationLevelName)
                .IsUnique()
                .HasDatabaseName("uq_education_level");
        });

        modelBuilder.Entity<DimStandardJobRoleDbModel>(b =>
        {
            b.ToTable("dim_standard_job_role");
            b.HasKey(j => j.StandardJobRoleId);
            b.Property(j => j.StandardJobRoleId)
                .HasColumnName("standard_job_role_id")
                .UseIdentityByDefaultColumn();
            b.Property(j => j.StandardJobRoleTitle)
                .HasColumnName("standard_job_role_title")
                .HasMaxLength(255)
                .IsRequired();
            b.HasIndex(j => j.StandardJobRoleTitle)
                .IsUnique()
                .HasDatabaseName("uq_dim_sjr_title");
            b.Property(j => j.IndustryFieldId)
                .HasColumnName("industry_field_id")
                .IsRequired();
            b.HasOne(sjr => sjr.DimIndustryField)
                .WithMany(ifield => ifield.DimStandardJobRoles)
                .HasForeignKey(sjr => sjr.IndustryFieldId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimStandardJobRoleHierarchyDbModel>(b =>
        {
            b.ToTable("dim_standard_job_role_hierarchy");
            b.HasKey(sjh => new { sjh.StandardJobRoleId, sjh.HierarchyLevelId })
                .HasName("pk_dim_sjrh");
            b.Property(sjh => sjh.StandardJobRoleId)
                .HasColumnName("standard_job_role_id")
                .ValueGeneratedNever(); 
            b.Property(sjh => sjh.HierarchyLevelId)
                .HasColumnName("hierarchy_level_id")
                .ValueGeneratedNever(); 
            b.HasOne(sjh => sjh.StandardJobRole)
                .WithMany(sjr => sjr.DimStandardJobRoleHierarchies)
                .HasForeignKey(sjh => sjh.StandardJobRoleId)
                .HasConstraintName("fk_dim_sjrh_sjr")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(sjh => sjh.HierarchyLevel)
                .WithMany(hl => hl.DimStandardJobRoleHierarchies)
                .HasForeignKey(sjh => sjh.HierarchyLevelId)
                .HasConstraintName("fk_dim_sjrh_hl")
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
        
        modelBuilder.Entity<SalaryDistributionBucketDto>(dto =>
        {
            dto.HasNoKey();
        });
        
        modelBuilder.Entity<SalarySummaryDto>(dto =>
        {
            dto.HasNoKey();
        });

        modelBuilder.Entity<SalaryTimeSeriesPointDto>(dto =>
        {
            dto.HasNoKey();
        });
    }
}