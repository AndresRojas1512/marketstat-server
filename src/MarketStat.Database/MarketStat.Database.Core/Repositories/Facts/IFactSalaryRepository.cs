using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Etl;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;

namespace MarketStat.Database.Core.Repositories.Facts;

public interface IFactSalaryRepository
{
    // CRUD 
    Task AddFactSalaryAsync(FactSalary salary);
    Task<FactSalary> GetFactSalaryByIdAsync(long salaryId);
    Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync();
    Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(SalaryFilterDto salaryFilterDto);
    Task UpdateFactSalaryAsync(FactSalary salaryFact);
    Task DeleteFactSalaryByIdAsync(long salaryFactId);
    
    // Authorized analytical methods
    Task<List<SalaryDistributionBucketDto>> GetSalaryDistributionAsync(SalaryFilterDto filters);
    Task<SalarySummaryDto?> GetSalarySummaryAsync(SalaryFilterDto filters, int targetPercentile);
    Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(SalaryFilterDto filters,
        TimeGranularity granularity, int periods);
    
    // Public analytical methods
    Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesByLocationIndustryAsync(PublicRolesQueryDto queryDto);

    Task<IEnumerable<PublicSalaryByEducationInIndustryDto>> GetPublicSalaryByEducationInIndustryAsync(
        PublicSalaryByEducationQueryDto queryDto);
    Task<IEnumerable<PublicTopEmployerRoleSalariesInIndustryDto>> GetPublicTopEmployerRoleSalariesInIndustryAsync(
        PublicTopEmployerRoleSalariesQueryDto queryDto);
}