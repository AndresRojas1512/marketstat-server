using System.Text.Json;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;
using MarketStat.Common.Enums;

namespace MarketStat.Services.Account.BenchmarkHistoryService.Validator;

public static class BenchmarkHistoryValidator
{
    private const int MaxBenchmarkNameLength = 255;
    
    private static readonly string[] ValidGranularityStrings = 
        Enum.GetNames(typeof(TimeGranularity)).Select(s => s.ToLowerInvariant()).ToArray();

    public static void ValidateForSave(SaveBenchmarkRequestDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "Save benchmark request data cannot be null.");
        }

        // 1. BenchmarkName validation
        if (!string.IsNullOrWhiteSpace(dto.BenchmarkName) && dto.BenchmarkName.Length > MaxBenchmarkNameLength)
        {
            throw new ArgumentException($"Benchmark name cannot exceed {MaxBenchmarkNameLength} characters.", nameof(dto.BenchmarkName));
        }

        // 2. BenchmarkResultJson validation
        if (string.IsNullOrWhiteSpace(dto.BenchmarkResultJson))
        {
            throw new ArgumentException("Benchmark result JSON is required.", nameof(dto.BenchmarkResultJson));
        }
        
        try
        {
            using (JsonDocument.Parse(dto.BenchmarkResultJson))
            {
                // Valid JSON structure
            }
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Benchmark result is not valid JSON.", nameof(dto.BenchmarkResultJson), ex);
        }

        // 3. Date range validation
        if (dto.FilterDateStart.HasValue && dto.FilterDateEnd.HasValue && dto.FilterDateStart.Value > dto.FilterDateEnd.Value)
        {
            throw new ArgumentException("FilterDateStart cannot be after FilterDateEnd.", nameof(dto.FilterDateStart));
        }

        // 4. TargetPercentile validation
        if (dto.FilterTargetPercentile.HasValue && (dto.FilterTargetPercentile < 0 || dto.FilterTargetPercentile > 100))
        {
            throw new ArgumentException("FilterTargetPercentile must be between 0 and 100 if provided.", nameof(dto.FilterTargetPercentile));
        }

        // 5. Periods validation
        if (dto.FilterPeriods.HasValue && dto.FilterPeriods < 1)
        {
            throw new ArgumentException("FilterPeriods must be a positive integer if provided.", nameof(dto.FilterPeriods));
        }

        // 6. Validation for ID-based filters
        if (dto.FilterIndustryFieldId.HasValue && dto.FilterIndustryFieldId.Value <= 0)
        {
            throw new ArgumentException("FilterIndustryFieldId must be a positive integer if provided.", nameof(dto.FilterIndustryFieldId));
        }
        if (dto.FilterStandardJobRoleId.HasValue && dto.FilterStandardJobRoleId.Value <= 0)
        {
            throw new ArgumentException("FilterStandardJobRoleId must be a positive integer if provided.", nameof(dto.FilterStandardJobRoleId));
        }
        if (dto.FilterHierarchyLevelId.HasValue && dto.FilterHierarchyLevelId.Value <= 0)
        {
            throw new ArgumentException("FilterHierarchyLevelId must be a positive integer if provided.", nameof(dto.FilterHierarchyLevelId));
        }
        if (dto.FilterDistrictId.HasValue && dto.FilterDistrictId.Value <= 0)
        {
            throw new ArgumentException("FilterDistrictId must be a positive integer if provided.", nameof(dto.FilterDistrictId));
        }
        if (dto.FilterOblastId.HasValue && dto.FilterOblastId.Value <= 0)
        {
            throw new ArgumentException("FilterOblastId must be a positive integer if provided.", nameof(dto.FilterOblastId));
        }
        if (dto.FilterCityId.HasValue && dto.FilterCityId.Value <= 0)
        {
            throw new ArgumentException("FilterCityId must be a positive integer if provided.", nameof(dto.FilterCityId));
        }

        if (!string.IsNullOrWhiteSpace(dto.FilterGranularity) && 
            !ValidGranularityStrings.Contains(dto.FilterGranularity.ToLowerInvariant()))
        {
            throw new ArgumentException($"FilterGranularity, if provided, must be one of: {string.Join(", ", ValidGranularityStrings)} (case-insensitive).", nameof(dto.FilterGranularity));
        }
    }
}