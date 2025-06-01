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
    public Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(SalaryFilterDto salaryFilterDto);
    Task UpdateFactSalaryAsync(FactSalary salaryFact);
    Task DeleteFactSalaryByIdAsync(long salaryFactId);
    
    // Analytics
    Task<string?> GetBenchmarkingReportJsonAsync(BenchmarkQueryDto filters);
    Task<List<SalaryDistributionBucketDto>> GetSalaryDistributionAsync(SalaryFilterDto filters);
    Task<SalarySummaryDto?> GetSalarySummaryAsync(SalaryFilterDto filters, int targetPercentile);
    Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(SalaryFilterDto filters,
        TimeGranularity granularity, int periods);
    
    // Public Analytical Methods
    Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesByLocationIndustryAsync(
        int industryFieldId, 
        int? federalDistrictId, 
        int? oblastId, 
        int? cityId, 
        int minSalaryRecordsForRole);

    Task<IEnumerable<PublicDegreeByIndustryDto>> GetPublicTopDegreesByIndustryAsync(
        int industryFieldId, 
        int topNDegrees, 
        int minEmployeeCountForDegree);

    Task TruncateStagingTableAsync(string stagingTableName);
    Task BatchInsertToStagingTableAsync(string stagingTableName, IEnumerable<StagedSalaryRecordDto> records);
    Task<(int insertedCount, int skippedCount)> CallBulkLoadFromStagingProcedureAsync(string stagingTableName);
}