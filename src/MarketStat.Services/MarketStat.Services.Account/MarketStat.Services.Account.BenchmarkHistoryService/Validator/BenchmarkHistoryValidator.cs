using System.Text.Json;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;
using MarketStat.Common.Enums;

namespace MarketStat.Services.Account.BenchmarkHistoryService.Validator;

public static class BenchmarkHistoryValidator
{
    private const int MaxBenchmarkNameLength = 255;
    private const int MaxStringFilterLength = 255;
    
    private static readonly string[] ValidGranularityStrings = 
        Enum.GetNames(typeof(TimeGranularity)).Select(s => s.ToLowerInvariant()).ToArray();

    public static void ValidateForSave(SaveBenchmarkRequestDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "Save benchmark request data cannot be null.");
        }

        if (!string.IsNullOrWhiteSpace(dto.BenchmarkName) && dto.BenchmarkName.Length > MaxBenchmarkNameLength)
        {
            throw new ArgumentException($"Benchmark name cannot exceed {MaxBenchmarkNameLength} characters.", nameof(dto.BenchmarkName));
        }

        if (string.IsNullOrWhiteSpace(dto.BenchmarkResultJson))
        {
            throw new ArgumentException("Benchmark result JSON is required.", nameof(dto.BenchmarkResultJson));
        }
        try
        {
            using (JsonDocument.Parse(dto.BenchmarkResultJson))
            {
            }
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Benchmark result is not valid JSON.", nameof(dto.BenchmarkResultJson), ex);
        }

        if (dto.FilterDateStart.HasValue && dto.FilterDateEnd.HasValue && dto.FilterDateStart.Value > dto.FilterDateEnd.Value)
            throw new ArgumentException("FilterDateStart cannot be after FilterDateEnd.", nameof(dto.FilterDateStart));

        if (dto.FilterTargetPercentile.HasValue && (dto.FilterTargetPercentile < 0 || dto.FilterTargetPercentile > 100))
            throw new ArgumentException("FilterTargetPercentile must be between 0 and 100 if provided.", nameof(dto.FilterTargetPercentile));

        if (dto.FilterPeriods.HasValue && dto.FilterPeriods < 1)
            throw new ArgumentException("FilterPeriods must be a positive integer if provided.", nameof(dto.FilterPeriods));

        if (dto.FilterStandardJobRoleTitle?.Length > MaxStringFilterLength)
            throw new ArgumentException($"FilterStandardJobRoleTitle cannot exceed {MaxStringFilterLength} characters.",
                nameof(dto.FilterStandardJobRoleTitle));
        if (dto.FilterHierarchyLevelName?.Length > MaxStringFilterLength)
            throw new ArgumentException($"FilterHierarchyLevelName cannot exceed {MaxStringFilterLength} characters.",
                nameof(dto.FilterHierarchyLevelName));
        if (dto.FilterDistrictName?.Length > MaxStringFilterLength)
            throw new ArgumentException($"FilterDistrictName cannot exceed {MaxStringFilterLength} characters.",
                nameof(dto.FilterDistrictName));
        if (dto.FilterOblastName?.Length > MaxStringFilterLength)
            throw new ArgumentException($"FilterOblastName cannot exceed {MaxStringFilterLength} characters.",
                nameof(dto.FilterOblastName));
        if (dto.FilterCityName?.Length > MaxStringFilterLength)
            throw new ArgumentException($"FilterCityName cannot exceed {MaxStringFilterLength} characters.",
                nameof(dto.FilterCityName));
        

        if (dto.FilterIndustryFieldId.HasValue && dto.FilterIndustryFieldId.Value <= 0)
        {
            throw new ArgumentException("FilterIndustryFieldId must be a positive integer if provided.", nameof(dto.FilterIndustryFieldId));
        }

        if (!string.IsNullOrWhiteSpace(dto.FilterGranularity) && 
            !ValidGranularityStrings.Contains(dto.FilterGranularity.ToLowerInvariant()))
        {
            throw new ArgumentException($"FilterGranularity, if provided, must be one of: {string.Join(", ", ValidGranularityStrings)} (case-insensitive).", nameof(dto.FilterGranularity));
        }
    }
}