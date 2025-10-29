using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Etl;
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
    Task<BenchmarkDataDto?> GetBenchmarkingReportAsync(BenchmarkQueryDto benchmarkFilters);
    
    // Public Analytical Methods
    // Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesByLocationIndustryAsync(
    //      PublicRolesQueryDto queryDto);
    //
    // Task<IEnumerable<PublicSalaryByEducationInIndustryDto>> GetPublicSalaryByEducationInIndustryAsync(
    //     PublicSalaryByEducationQueryDto queryDto);
    //
    // Task<IEnumerable<PublicTopEmployerRoleSalariesInIndustryDto>> GetPublicTopEmployerRoleSalariesInIndustryAsync(
    //     PublicTopEmployerRoleSalariesQueryDto queryDto);
    
    // ETL Methods
}