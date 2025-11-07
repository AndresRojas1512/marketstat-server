using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Responses;

namespace MarketStat.Services.Facts.FactSalaryService;

public interface IFactSalaryService
{
    // CRUD
    Task<FactSalary> CreateFactSalaryAsync(int dateId, int locationId, int employerId, int jobId, int employeeId,
        decimal salaryAmount);
    Task<FactSalary> GetFactSalaryByIdAsync(long salaryFactId);
    Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(AnalysisFilterRequest request);
    Task<FactSalary> UpdateFactSalaryAsync(long salaryFactId, int dateId, int locationId, int employerId, int jobId, 
        int employeeId, decimal salaryAmount);
    Task DeleteFactSalaryAsync(long salaryFactId);
    
    // Authorized Analytics
    Task<List<SalaryDistributionBucket>> GetSalaryDistributionAsync(AnalysisFilterRequest request);
    Task<SalarySummary?> GetSalarySummaryAsync(SalarySummaryRequest request);
    Task<List<SalaryTimeSeriesPoint>> GetSalaryTimeSeriesAsync(TimeSeriesRequest request);
    
    // Public Analytics
    Task<IEnumerable<PublicRoleByLocationIndustry>> GetPublicRolesAsync(PublicRolesRequest request);
}