using MarketStat.Common.Core.Facts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.Keyless;

public class FactSalaryKeylessConfiguration : IEntityTypeConfiguration<FactSalary>
{
    public void Configure(EntityTypeBuilder<FactSalary> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.HasNoKey();
        builder.ToTable((string?)null);
    }
}
