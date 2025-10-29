using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;

namespace MarketStat.Database.Core.Repositories.Facts;

public interface IFactSalaryRepository
{
    // CRUD 
    Task AddFactSalaryAsync(FactSalary salary);
    Task<FactSalary> GetFactSalaryByIdAsync(long salaryId);
    Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(ResolvedSalaryFilterDto resolvedFilters);
    Task UpdateFactSalaryAsync(FactSalary salaryFact);
    Task DeleteFactSalaryByIdAsync(long salaryFactId);
    
    // Authorized analytical methods
    Task<List<SalaryDistributionBucketDto>> GetSalaryDistributionAsync(ResolvedSalaryFilterDto resolvedFilters);
    Task<SalarySummaryDto?> GetSalarySummaryAsync(ResolvedSalaryFilterDto resolvedFilters, int targetPercentile);
    Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(ResolvedSalaryFilterDto resolvedFilters,
        TimeGranularity granularity, int periods);
    
    // Public analytical methods
    // Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesByLocationIndustryAsync(PublicRolesQueryDto queryDto);
    //
    // Task<IEnumerable<PublicSalaryByEducationInIndustryDto>> GetPublicSalaryByEducationInIndustryAsync(
    //     PublicSalaryByEducationQueryDto queryDto);
    // Task<IEnumerable<PublicTopEmployerRoleSalariesInIndustryDto>> GetPublicTopEmployerRoleSalariesInIndustryAsync(
    //     PublicTopEmployerRoleSalariesQueryDto queryDto);
}