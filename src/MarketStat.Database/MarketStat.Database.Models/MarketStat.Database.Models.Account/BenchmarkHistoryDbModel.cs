using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models.MarketStat.Database.Models.Account;

[Table("benchmark_history")]
public class BenchmarkHistoryDbModel
{
    [Key]
    [Column("benchmark_history_id")]
    public long BenchmarkHistoryId { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("benchmark_name")]
    [StringLength(255)]
    public string? BenchmarkName { get; set; }

    [Required]
    [Column("saved_at")]
    public DateTimeOffset SavedAt { get; set; }

    [Column("filter_industry_field_name")]
    public string? FilterIndustryFieldName { get; set; }

    [Column("filter_standard_job_role_title")]
    public string? FilterStandardJobRoleTitle { get; set; }

    [Column("filter_hierarchy_level_name")]
    public string? FilterHierarchyLevelName { get; set; }

    [Column("filter_district_name")]
    public string? FilterDistrictName { get; set; }

    [Column("filter_oblast_name")]
    public string? FilterOblastName { get; set; }

    [Column("filter_city_name")]
    public string? FilterCityName { get; set; }

    [Column("filter_date_start", TypeName = "date")]
    public DateOnly? FilterDateStart { get; set; }

    [Column("filter_date_end", TypeName = "date")]
    public DateOnly? FilterDateEnd { get; set; }

    [Column("filter_target_percentile")]
    public int? FilterTargetPercentile { get; set; }

    [Column("filter_granularity")]
    public string? FilterGranularity { get; set; } // TEXT

    [Column("filter_periods")]
    public int? FilterPeriods { get; set; }

    [Required]
    [Column("benchmark_result_json", TypeName = "jsonb")]
    public string BenchmarkResultJson { get; set; } = "{}";

    [ForeignKey(nameof(UserId))]
    public virtual UserDbModel? User { get; set; }

    public BenchmarkHistoryDbModel()
    {
    }
}