using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.Dimensions;

public class DimLocationConfiguration : IEntityTypeConfiguration<DimLocationDbModel>
{
    public void Configure(EntityTypeBuilder<DimLocationDbModel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("dim_location");
        builder.HasKey(l => l.LocationId);
        builder.Property(l => l.LocationId).HasColumnName("location_id").UseIdentityByDefaultColumn();

        builder.Property(l => l.CityName).HasColumnName("city_name").HasMaxLength(255).IsRequired();
        builder.Property(l => l.OblastName).HasColumnName("oblast_name").HasMaxLength(255).IsRequired();
        builder.Property(l => l.DistrictName).HasColumnName("district_name").HasMaxLength(255).IsRequired();

        builder.HasIndex(l => new { l.CityName, l.OblastName, l.DistrictName })
            .IsUnique()
            .HasDatabaseName("uq_dim_location");
    }
}
