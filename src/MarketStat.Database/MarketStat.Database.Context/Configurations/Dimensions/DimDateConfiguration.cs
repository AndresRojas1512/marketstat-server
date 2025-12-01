using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.Dimensions;

public class DimDateConfiguration : IEntityTypeConfiguration<DimDateDbModel>
{
    public void Configure(EntityTypeBuilder<DimDateDbModel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("dim_date", tb =>
        {
            tb.HasCheckConstraint("CK_dim_date_quarter", "\"quarter\" BETWEEN 1 AND 4");
            tb.HasCheckConstraint("CK_dim_date_month", "\"month\" BETWEEN 1 AND 12");
        });
        builder.HasKey(d => d.DateId);
        builder.Property(d => d.DateId).HasColumnName("date_id").UseIdentityByDefaultColumn();

        builder.Property(d => d.FullDate).HasColumnName("full_date").HasColumnType("date").IsRequired();
        builder.HasIndex(d => d.FullDate).IsUnique();

        builder.Property(d => d.Year).HasColumnName("year").IsRequired();
        builder.Property(d => d.Quarter).HasColumnName("quarter").IsRequired();
        builder.Property(d => d.Month).HasColumnName("month").IsRequired();
    }
}
