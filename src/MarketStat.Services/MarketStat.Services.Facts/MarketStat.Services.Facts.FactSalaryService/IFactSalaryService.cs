using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;

namespace MarketStat.Services.Facts.FactSalaryService;

public interface IFactSalaryService
{
    // CRUD
    Task<FactSalary> CreateFactSalaryAsync(int dateId, int cityId, int employerId, int jobRoleId, int employeeId,
        decimal salaryAmount, decimal bonusAmount);
    Task<FactSalary?> GetFactSalaryByIdAsync(long salaryFactId);
    Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync();
    Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(SalaryFilterDto filterDto);
    Task<FactSalary> UpdateFactSalaryAsync(long salaryFactId, int dateId, int cityId, int employerId, int jobRoleId,
        int employeeId, decimal salaryAmount, decimal bonusAmount);
    Task DeleteFactSalaryAsync(int salaryFactId);
    
    // Analytics
    Task<BenchmarkDataDto?> GetBenchmarkingReportAsync(BenchmarkQueryDto filters);
    Task<List<SalaryDistributionBucketDto>> GetSalaryDistributionAsync(SalaryFilterDto filters);
    Task<SalarySummaryDto?> GetSalarySummaryAsync(SalaryFilterDto filters, int targetPercentile);
    Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(SalaryFilterDto filters, TimeGranularity granularity, int periods);
}