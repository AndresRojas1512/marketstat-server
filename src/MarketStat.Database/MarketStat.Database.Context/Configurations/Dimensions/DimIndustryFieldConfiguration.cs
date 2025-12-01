using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.Dimensions;

public class DimIndustryFieldConfiguration : IEntityTypeConfiguration<DimIndustryFieldDbModel>
{
    public void Configure(EntityTypeBuilder<DimIndustryFieldDbModel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("dim_industry_field");
        builder.HasKey(i => i.IndustryFieldId);
        builder.Property(i => i.IndustryFieldId).HasColumnName("industry_field_id").UseIdentityByDefaultColumn();

        builder.Property(i => i.IndustryFieldCode).HasColumnName("industry_field_code").HasMaxLength(10).IsRequired();
        builder.HasIndex(i => i.IndustryFieldCode).IsUnique().HasDatabaseName("uq_dim_industry_field_code");

        builder.Property(i => i.IndustryFieldName).HasColumnName("industry_field_name").HasMaxLength(255).IsRequired();
        builder.HasIndex(i => i.IndustryFieldName).IsUnique().HasDatabaseName("uq_dim_industry_field_name");
    }
}
