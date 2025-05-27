using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;

public class SaveBenchmarkRequestDto
{
    [StringLength(255, ErrorMessage = "Benchmark name cannot exceed 255 characters.")]
    public string? BenchmarkName { get; set; }

    public int? FilterIndustryFieldId { get; set; }
    public int? FilterStandardJobRoleId { get; set; }
    public int? FilterHierarchyLevelId { get; set; }
    public int? FilterDistrictId { get; set; }
    public int? FilterOblastId { get; set; }
    public int? FilterCityId { get; set; }
    public DateOnly? FilterDateStart { get; set; }
    public DateOnly? FilterDateEnd { get; set; }
        
    [Range(0, 100, ErrorMessage = "Target percentile must be between 0 and 100 if provided.")]
    public int? FilterTargetPercentile { get; set; }

    public string? FilterGranularity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Periods must be a positive integer if provided.")]
    public int? FilterPeriods { get; set; }

    [Required(ErrorMessage = "Benchmark result JSON is required.")]
    public string BenchmarkResultJson { get; set; } = string.Empty;
}