using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Models.MarketStat.Database.Models.Facts;

[Keyless]
public class SalaryStatsDbModel
{
    [Column("count")] public long Count { get; init; }
    [Column("min")] public decimal Min { get; init; }
    [Column("max")] public decimal Max { get; init; }
    [Column("mean")] public decimal Mean { get; init; }
    [Column("median")] public decimal Median { get; init; }
    [Column("p25")] public decimal Percentile25 { get; init; }
    [Column("p75")] public decimal Percentile75 { get; init; }
}