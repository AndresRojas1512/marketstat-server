using MarketStat.Common.Dto.Facts.Analytics.Payloads;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.Keyless;

public class PublicRoleConfiguration : IEntityTypeConfiguration<PublicRoleByLocationIndustryDto>
{
    public void Configure(EntityTypeBuilder<PublicRoleByLocationIndustryDto> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.HasNoKey();
        builder.ToTable((string?)null);
    }
}
