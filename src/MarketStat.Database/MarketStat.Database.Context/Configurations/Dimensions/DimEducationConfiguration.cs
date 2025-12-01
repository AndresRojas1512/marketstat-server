using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.Dimensions;

public class DimEducationConfiguration : IEntityTypeConfiguration<DimEducationDbModel>
{
    public void Configure(EntityTypeBuilder<DimEducationDbModel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("dim_education");
        builder.HasKey(e => e.EducationId);
        builder.Property(e => e.EducationId).HasColumnName("education_id").UseIdentityByDefaultColumn();

        builder.Property(e => e.SpecialtyName).HasColumnName("specialty_name").HasMaxLength(255).IsRequired();
        builder.Property(e => e.SpecialtyCode).HasColumnName("specialty_code").HasMaxLength(255).IsRequired();
        builder.Property(e => e.EducationLevelName).HasColumnName("education_level_name").HasMaxLength(255).IsRequired();

        builder.HasIndex(e => new { e.SpecialtyName, e.EducationLevelName })
            .IsUnique()
            .HasDatabaseName("uq_dim_education");
    }
}
