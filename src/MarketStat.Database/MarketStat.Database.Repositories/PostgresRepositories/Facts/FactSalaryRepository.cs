using System.Data;
using System.Text;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Linq;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;

namespace MarketStat.Database.Repositories.PostgresRepositories.Facts;

public class FactSalaryRepository : IFactSalaryRepository
{
    private readonly MarketStatDbContext _dbContext;
    private readonly ILogger<FactSalaryRepository> _logger;

    public FactSalaryRepository(MarketStatDbContext dbContext, ILogger<FactSalaryRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task AddFactSalaryAsync(FactSalary salary)
    {
        var dbModel = FactSalaryConverter.ToDbModel(salary);
        
        await _dbContext.FactSalaries.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg &&
                  pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                "One or more referenced entities (date, city, employer, job role or employee) were not found when adding salary fact.");
        }
        salary.SalaryFactId = dbModel.SalaryFactId;
    }

    public async Task<FactSalary> GetFactSalaryByIdAsync(long salaryId)
    {
        var dbSalary = await _dbContext.FactSalaries
            .AsNoTracking()
            .FirstOrDefaultAsync(fs => fs.SalaryFactId == salaryId);

        if (dbSalary == null)
        {
            throw new NotFoundException($"Salary fact with ID {salaryId} not found.");
        }
        return FactSalaryConverter.ToDomain(dbSalary);
    }

    public async Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync()
    {
        var allDbModels = await _dbContext.FactSalaries.AsNoTracking().ToListAsync();
        return allDbModels.Select(FactSalaryConverter.ToDomain);
    }

    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(SalaryFilterDto filterDto)
    {
        var sql = $"SELECT " +
                  $"    salary_fact_id AS \"SalaryFactId\", " +
                  $"    date_id AS \"DateId\", " +
                  $"    city_id_from_fact AS \"CityId\", " +
                  $"    employer_id AS \"EmployerId\", " +
                  $"    job_role_id_from_fact AS \"JobRoleId\", " +
                  $"    employee_id AS \"EmployeeId\", " +
                  $"    salary_amount AS \"SalaryAmount\", " +
                  $"    bonus_amount AS \"BonusAmount\" " +
                  $"FROM marketstat.fn_filtered_salaries(" +
                  $"{filterDto.IndustryFieldId}, {filterDto.StandardJobRoleId}, {filterDto.HierarchyLevelId}, " +
                  $"{filterDto.DistrictId}, {filterDto.OblastId}, {filterDto.CityId}, " +
                  $"{filterDto.DateStart}, {filterDto.DateEnd})";
        
        var results = await _dbContext.Set<FactSalary>()
            .FromSqlInterpolated($"SELECT salary_fact_id, date_id, city_id_from_fact AS CityId, employer_id, job_role_id_from_fact AS JobRoleId, employee_id, salary_amount, bonus_amount FROM marketstat.fn_filtered_salaries({filterDto.IndustryFieldId}, {filterDto.StandardJobRoleId}, {filterDto.HierarchyLevelId}, {filterDto.DistrictId}, {filterDto.OblastId}, {filterDto.CityId}, {filterDto.DateStart}, {filterDto.DateEnd})")
            .AsNoTracking()
            .ToListAsync();
        
        return results;
    }

    public async Task UpdateFactSalaryAsync(FactSalary salaryFact)
    {
        var dbModel = await _dbContext.FactSalaries.FirstOrDefaultAsync(fs => fs.SalaryFactId == salaryFact.SalaryFactId);
        if (dbModel == null)
            throw new NotFoundException($"Salary fact with ID {salaryFact.SalaryFactId} not found for update.");

        dbModel.DateId = salaryFact.DateId;
        dbModel.CityId = salaryFact.CityId;
        dbModel.EmployerId = salaryFact.EmployerId;
        dbModel.JobRoleId = salaryFact.JobRoleId;
        dbModel.EmployeeId = salaryFact.EmployeeId;
        dbModel.SalaryAmount = salaryFact.SalaryAmount;
        dbModel.BonusAmount = salaryFact.BonusAmount;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx &&
                  pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                "One or more referenced entities (date, city, employer, job role or employee) were not found during update.");
        }
    }

    public async Task DeleteFactSalaryByIdAsync(long salaryFactId)
    {
        var dbModel = await _dbContext.FactSalaries.FindAsync(salaryFactId);
        if (dbModel == null)
            throw new NotFoundException($"Salary fact with ID {salaryFactId} not found for deletion.");

        _dbContext.FactSalaries.Remove(dbModel);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<string?> GetBenchmarkingReportJsonAsync(BenchmarkQueryDto filters)
    {
        _logger.LogInformation("GetBenchmarkingReportJsonAsync called with filters: {@Filters}", filters);

        string granularityString = filters.Granularity.ToString().ToLowerInvariant();
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("p_industry_field_id", NpgsqlDbType.Integer) { Value = (object?)filters.IndustryFieldId ?? DBNull.Value },
            new NpgsqlParameter("p_standard_job_role_id", NpgsqlDbType.Integer) { Value = (object?)filters.StandardJobRoleId ?? DBNull.Value },
            new NpgsqlParameter("p_hierarchy_level_id", NpgsqlDbType.Integer) { Value = (object?)filters.HierarchyLevelId ?? DBNull.Value },
            new NpgsqlParameter("p_district_id", NpgsqlDbType.Integer) { Value = (object?)filters.DistrictId ?? DBNull.Value },
            new NpgsqlParameter("p_oblast_id", NpgsqlDbType.Integer) { Value = (object?)filters.OblastId ?? DBNull.Value },
            new NpgsqlParameter("p_city_id", NpgsqlDbType.Integer) { Value = (object?)filters.CityId ?? DBNull.Value },
            new NpgsqlParameter("p_date_start", NpgsqlDbType.Date) { Value = (object?)filters.DateStart ?? DBNull.Value },
            new NpgsqlParameter("p_date_end", NpgsqlDbType.Date) { Value = (object?)filters.DateEnd ?? DBNull.Value },
            new NpgsqlParameter("p_target_percentile", NpgsqlDbType.Integer) { Value = filters.TargetPercentile },
            new NpgsqlParameter("p_granularity", NpgsqlDbType.Text) { Value = granularityString },
            new NpgsqlParameter("p_periods", NpgsqlDbType.Integer) { Value = filters.Periods }
        };

        var sql = @"SELECT marketstat.fn_compute_benchmark_data(
                        p_industry_field_id            := @p_industry_field_id,
                        p_standard_job_role_id         := @p_standard_job_role_id,
                        p_hierarchy_level_id           := @p_hierarchy_level_id,
                        p_district_id                  := @p_district_id,
                        p_oblast_id                    := @p_oblast_id,
                        p_city_id                      := @p_city_id,
                        p_date_start                   := @p_date_start,
                        p_date_end                     := @p_date_end,
                        p_target_percentile            := @p_target_percentile,
                        p_granularity                  := @p_granularity,
                        p_periods                      := @p_periods
                    );";

        _logger.LogDebug("Executing SQL for benchmark report: {Sql}", sql);
        foreach (var p in parameters)
        {
            // For DBNull.Value, log "<DBNull>" or similar to avoid issues with logging DBNull itself
            var paramValueForLog = (p.Value == DBNull.Value) ? "<DBNull>" : p.Value;
            _logger.LogDebug("Parameter: {ParameterName} = {ParameterValue} (NpgsqlDbType: {NpgsqlDbType})", p.ParameterName, paramValueForLog, p.NpgsqlDbType);
        }

        string? jsonResult = null;
        var connection = _dbContext.Database.GetDbConnection();
        var closeConnection = false;
        try
        {
            if (connection.State != ConnectionState.Open) { await connection.OpenAsync(); closeConnection = true; }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.AddRange(parameters.ToArray());
                var scalarResult = await command.ExecuteScalarAsync();
                if (scalarResult != null && scalarResult != DBNull.Value)
                {
                    jsonResult = scalarResult as string;
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error executing GetBenchmarkingReportJsonAsync SQL call.");
            throw; // Re-throw the exception to be handled by the service/middleware
        }
        finally
        {
            if (closeConnection && connection.State == ConnectionState.Open) { await connection.CloseAsync(); }
        }
        _logger.LogInformation("GetBenchmarkingReportJsonAsync raw JSON result: {JsonResult}", jsonResult);
        return jsonResult;
    }

    public async Task<List<SalaryDistributionBucketDto>> GetSalaryDistributionAsync(SalaryFilterDto filters)
    {
        return await _dbContext.Set<SalaryDistributionBucketDto>()
            .FromSqlInterpolated($"SELECT * FROM marketstat.fn_salary_distribution(p_source_temp_table_name := NULL, p_industry_field_id := {filters.IndustryFieldId}, p_standard_job_role_id := {filters.StandardJobRoleId}, p_hierarchy_level_id := {filters.HierarchyLevelId}, p_district_id := {filters.DistrictId}, p_oblast_id := {filters.OblastId}, p_city_id := {filters.CityId}, p_date_start := {filters.DateStart}, p_date_end := {filters.DateEnd})")
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SalarySummaryDto?> GetSalarySummaryAsync(SalaryFilterDto filters, int targetPercentile)
    {
        return await _dbContext.Set<SalarySummaryDto>()
            .FromSqlInterpolated($"SELECT * FROM marketstat.fn_salary_summary(p_source_temp_table_name := NULL, p_industry_field_id := {filters.IndustryFieldId}, p_standard_job_role_id := {filters.StandardJobRoleId}, p_hierarchy_level_id := {filters.HierarchyLevelId}, p_district_id := {filters.DistrictId}, p_oblast_id := {filters.OblastId}, p_city_id := {filters.CityId}, p_date_start := {filters.DateStart}, p_date_end := {filters.DateEnd}, p_target_percentile := {targetPercentile})")
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(SalaryFilterDto filters, TimeGranularity granularity, int periods)
    {
        string granularityString = granularity.ToString().ToLowerInvariant();
        return await _dbContext.Set<SalaryTimeSeriesPointDto>()
            .FromSqlInterpolated($"SELECT * FROM marketstat.fn_salary_time_series(p_source_temp_table_name := NULL, p_industry_field_id := {filters.IndustryFieldId}, p_standard_job_role_id := {filters.StandardJobRoleId}, p_hierarchy_level_id := {filters.HierarchyLevelId}, p_district_id := {filters.DistrictId}, p_oblast_id := {filters.OblastId}, p_city_id := {filters.CityId}, p_filter_date_start := {filters.DateStart}, p_filter_date_end := {filters.DateEnd}, p_granularity := {granularityString}, p_periods := {periods})")
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesByLocationIndustryAsync(
        int industryFieldId, 
        int? federalDistrictId, 
        int? oblastId, 
        int? cityId, 
        int minSalaryRecordsForRole)
    {
        return await _dbContext.Set<PublicRoleByLocationIndustryDto>()
            .FromSqlInterpolated($"SELECT * FROM marketstat.fn_public_get_roles_by_location_industry({industryFieldId}, {federalDistrictId}, {oblastId}, {cityId}, {minSalaryRecordsForRole})")
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<IEnumerable<PublicDegreeByIndustryDto>> GetPublicTopDegreesByIndustryAsync(
        int industryFieldId, 
        int topNDegrees, 
        int minEmployeeCountForDegree)
    {
        return await _dbContext.Set<PublicDegreeByIndustryDto>()
            .FromSqlInterpolated($"SELECT * FROM marketstat.fn_public_top_degrees_by_industry({industryFieldId}, {topNDegrees}, {minEmployeeCountForDegree})")
            .AsNoTracking()
            .ToListAsync();
    }
}