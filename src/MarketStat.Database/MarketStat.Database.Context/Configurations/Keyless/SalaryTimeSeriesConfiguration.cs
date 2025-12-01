using MarketStat.Common.Dto.Facts.Analytics.Payloads;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.Keyless;

public class SalaryTimeSeriesConfiguration : IEntityTypeConfiguration<SalaryTimeSeriesPointDto>
{
    public void Configure(EntityTypeBuilder<SalaryTimeSeriesPointDto> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.HasNoKey();
        builder.ToTable((string?)null);
    }
}
