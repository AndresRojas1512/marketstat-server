using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;
using Microsoft.AspNetCore.Http;

namespace MarketStat.Services.Facts.FactSalaryService;

public interface IFactSalaryService
{
    // CRUD
    Task<FactSalary> CreateFactSalaryAsync(int dateId, int locationId, int employerId, int jobId, int employeeId,
        decimal salaryAmount);
    Task<FactSalary> GetFactSalaryByIdAsync(long salaryFactId);
    Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(SalaryFilterDto filterDto);
    Task<FactSalary> UpdateFactSalaryAsync(long salaryFactId, int dateId, int locationId, int employerId, int jobId, 
        int employeeId, decimal salaryAmount);
    Task DeleteFactSalaryAsync(long salaryFactId);
    
    // Authorized Analytics
    Task<List<SalaryDistributionBucketDto>> GetSalaryDistributionAsync(SalaryFilterDto filters);
    Task<SalarySummaryDto?> GetSalarySummaryAsync(SalaryFilterDto filters, int targetPercentile);
    Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(SalaryFilterDto filters, TimeGranularity granularity,
        int periods);
    
    // Public Analytics
    Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesAsync(SalaryFilterDto userFilters,
        int minRecordCount);
}