using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.Dimensions;

public class DimEmployeeConfiguration : IEntityTypeConfiguration<DimEmployeeDbModel>
{
    public void Configure(EntityTypeBuilder<DimEmployeeDbModel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("dim_employee", tb =>
        {
            tb.HasCheckConstraint("ck_career_after_birth", "\"career_start_date\" > \"birth_date\"");
            tb.HasCheckConstraint("ck_career_min_age", "\"career_start_date\" >= \"birth_date\" + INTERVAL '16 years'");
            tb.HasCheckConstraint("ck_dim_emp_birth_date", "\"birth_date\" <= CURRENT_DATE");
            tb.HasCheckConstraint("ck_dim_emp_career_start", "\"career_start_date\" <= CURRENT_DATE");
        });

        builder.HasKey(e => e.EmployeeId);
        builder.Property(e => e.EmployeeId).HasColumnName("employee_id").UseIdentityByDefaultColumn();

        builder.Property(e => e.EmployeeRefId).HasColumnName("employee_ref_id").HasMaxLength(255).IsRequired();
        builder.HasIndex(e => e.EmployeeRefId).IsUnique().HasDatabaseName("uq_dim_employee_ref_id");

        builder.Property(e => e.BirthDate).HasColumnName("birth_date").HasColumnType("date").IsRequired();
        builder.Property(e => e.CareerStartDate).HasColumnName("career_start_date").HasColumnType("date").IsRequired();

        builder.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(50).IsRequired(false);
        builder.Property(e => e.GraduationYear).HasColumnName("graduation_year").IsRequired(false);
        builder.Property(e => e.EducationId).HasColumnName("education_id").IsRequired(false);

        builder.HasOne(e => e.Education)
            .WithMany(edu => edu.DimEmployees)
            .HasForeignKey(e => e.EducationId)
            .HasConstraintName("fk_dim_employee_education")
            .OnDelete(DeleteBehavior.SetNull);
    }
}
