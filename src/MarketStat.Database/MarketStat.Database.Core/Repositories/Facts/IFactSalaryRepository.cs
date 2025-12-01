namespace MarketStat.Database.Core.Repositories.Facts;

using MarketStat.Common.Core.Facts;
using MarketStat.Common.Core.Facts.Analytics.Responses;
using MarketStat.Common.Enums;

public interface IFactSalaryRepository
{
    // CRUD
    Task AddFactSalaryAsync(FactSalary salary);

    Task<FactSalary> GetFactSalaryByIdAsync(long salaryId);

    Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(ResolvedSalaryFilter resolvedFilters);

    Task UpdateFactSalaryAsync(FactSalary salaryFact);

    Task DeleteFactSalaryByIdAsync(long salaryFactId);

    // Authorized analytical methods
    Task<List<SalaryDistributionBucket>> GetSalaryDistributionAsync(ResolvedSalaryFilter resolvedFilters);

    Task<SalarySummary?> GetSalarySummaryAsync(ResolvedSalaryFilter resolvedFilters, int targetPercentile);

    Task<List<SalaryTimeSeriesPoint>> GetSalaryTimeSeriesAsync(
        ResolvedSalaryFilter resolvedFilters,
        TimeGranularity granularity,
        int periods);

    // Public analytical methods
    Task<IEnumerable<PublicRoleByLocationIndustry>> GetPublicRolesAsync(
        ResolvedSalaryFilter resolvedFilters,
        int minRecordCount);
}
