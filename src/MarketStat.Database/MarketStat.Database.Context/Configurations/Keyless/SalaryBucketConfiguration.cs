using MarketStat.Common.Dto.Facts.Analytics.Payloads;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.Keyless;

public class SalaryBucketConfiguration : IEntityTypeConfiguration<SalaryDistributionBucketDto>
{
    public void Configure(EntityTypeBuilder<SalaryDistributionBucketDto> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.HasNoKey();
        builder.ToTable((string?)null);
    }
}
