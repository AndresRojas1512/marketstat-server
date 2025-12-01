using MarketStat.Database.Models.Facts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.Facts;

public class FactSalaryConfiguration : IEntityTypeConfiguration<FactSalaryDbModel>
{
    public void Configure(EntityTypeBuilder<FactSalaryDbModel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("fact_salaries");
        builder.HasKey(s => s.SalaryFactId);
        builder.Property(s => s.SalaryFactId).HasColumnName("salary_fact_id").UseIdentityByDefaultColumn();

        builder.Property(x => x.DateId).HasColumnName("date_id").IsRequired();
        builder.Property(x => x.LocationId).HasColumnName("location_id").IsRequired();
        builder.Property(x => x.EmployerId).HasColumnName("employer_id").IsRequired();
        builder.Property(x => x.JobId).HasColumnName("job_id").IsRequired();
        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").IsRequired();
        builder.Property(x => x.SalaryAmount).HasColumnName("salary_amount").HasColumnType("numeric(18,2)").IsRequired();

        builder.HasOne(fs => fs.DimDate).WithMany(d => d.FactSalaries).HasForeignKey(fs => fs.DateId).HasConstraintName("fk_fact_date").OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(fs => fs.DimLocation).WithMany(l => l.FactSalaries).HasForeignKey(fs => fs.LocationId).HasConstraintName("fk_fact_location").OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(fs => fs.DimEmployer).WithMany(e => e.FactSalaries).HasForeignKey(fs => fs.EmployerId).HasConstraintName("fk_fact_employer").OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(fs => fs.DimJob).WithMany(j => j.FactSalaries).HasForeignKey(fs => fs.JobId).HasConstraintName("fk_fact_job").OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(fs => fs.DimEmployee).WithMany(e => e.FactSalaries).HasForeignKey(fs => fs.EmployeeId).HasConstraintName("fk_fact_employee").OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(fs => fs.DateId).HasDatabaseName("idx_fact_date");
        builder.HasIndex(fs => fs.LocationId).HasDatabaseName("idx_fact_location");
        builder.HasIndex(fs => fs.EmployerId).HasDatabaseName("idx_fact_employer");
        builder.HasIndex(fs => fs.JobId).HasDatabaseName("idx_fact_job");
        builder.HasIndex(fs => fs.EmployeeId).HasDatabaseName("idx_fact_employee");
    }
}
