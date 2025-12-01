using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.Dimensions;

public class DimJobConfiguration : IEntityTypeConfiguration<DimJobDbModel>
{
    public void Configure(EntityTypeBuilder<DimJobDbModel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("dim_job");
        builder.HasKey(j => j.JobId);
        builder.Property(j => j.JobId).HasColumnName("job_id").UseIdentityByDefaultColumn();

        builder.Property(j => j.JobRoleTitle).HasColumnName("job_role_title").HasMaxLength(255).IsRequired();
        builder.Property(j => j.StandardJobRoleTitle).HasColumnName("standard_job_role_title").HasMaxLength(255).IsRequired();
        builder.Property(j => j.HierarchyLevelName).HasColumnName("hierarchy_level_name").HasMaxLength(255).IsRequired();
        builder.Property(j => j.IndustryFieldId).HasColumnName("industry_field_id").IsRequired();

        builder.HasIndex(j => new { j.JobRoleTitle, j.StandardJobRoleTitle, j.HierarchyLevelName, j.IndustryFieldId })
            .IsUnique()
            .HasDatabaseName("uq_dim_job");

        builder.HasOne(j => j.IndustryField)
            .WithMany(i => i.DimJobs)
            .HasForeignKey(j => j.IndustryFieldId)
            .HasConstraintName("fk_dim_job_industry")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
