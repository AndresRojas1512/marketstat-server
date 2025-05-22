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
using NpgsqlTypes;

namespace MarketStat.Database.Repositories.PostgresRepositories.Facts;

public class FactSalaryRepository : IFactSalaryRepository
{
    private readonly MarketStatDbContext _dbContext;

    public FactSalaryRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddFactSalaryAsync(FactSalary salary)
    {
        var dbModel = new FactSalaryDbModel(
            dateId: salary.DateId,
            cityId: salary.CityId,
            employerId: salary.EmployerId,
            jobRoleId: salary.JobRoleId,
            employeeId: salary.EmployeeId,
            salaryAmount: salary.SalaryAmount,
            bonusAmount: salary.BonusAmount
        );
        await _dbContext.FactSalaries.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg &&
                  pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException("One or more referenced entities (date, city, employer, job role or employee) were not found when adding salary fact.");
        }
        salary.SalaryFactId = dbModel.SalaryFactId;
    }

    public async Task<FactSalary> GetFactSalaryByIdAsync(long salaryId)
    {
        var dbSalary = await _dbContext.FactSalaries.FindAsync(salaryId);
        if (dbSalary is null)
            throw new NotFoundException($"Salary fact with ID {salaryId} not found.");
        return FactSalaryConverter.ToDomain(dbSalary);
    }

    public async Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync()
    {
        var allDbModels = await _dbContext.FactSalaries.AsNoTracking().ToListAsync();
        return allDbModels.Select(FactSalaryConverter.ToDomain);
    }

    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(SalaryFilterDto filterDto)
    {
        IQueryable<FactSalaryDbModel> query = _dbContext.FactSalaries.AsQueryable();
        
        if (filterDto.DateStart.HasValue)
        {
            query = query.Where(fs => fs.DimDate != null && fs.DimDate.FullDate >= filterDto.DateStart.Value);
        }
        if (filterDto.DateEnd.HasValue)
        {
            query = query.Where(fs => fs.DimDate != null && fs.DimDate.FullDate <= filterDto.DateEnd.Value);
        }

        if (filterDto.CityId.HasValue)
        {
            query = query.Where(fs => fs.CityId == filterDto.CityId.Value);
        }
        else if (filterDto.OblastId.HasValue)
        {
            query = query.Where(fs => fs.DimCity != null && fs.DimCity.OblastId == filterDto.OblastId.Value);
        }
        else if (filterDto.DistrictId.HasValue)
        {
            query = query.Where(fs => fs.DimCity != null && fs.DimCity.DimOblast != null &&
                                   fs.DimCity.DimOblast.DistrictId == filterDto.DistrictId.Value);
        }

        if (filterDto.StandardJobRoleId.HasValue)
        {
            query = query.Where(fs => fs.DimJobRole != null && fs.DimJobRole.StandardJobRoleId == filterDto.StandardJobRoleId.Value);
        }

        if (filterDto.HierarchyLevelId.HasValue)
        {
            query = query.Where(fs => fs.DimJobRole != null && fs.DimJobRole.HierarchyLevelId == filterDto.HierarchyLevelId.Value);
        }

        if (filterDto.IndustryFieldId.HasValue)
        {
            query = query.Where(fs => fs.DimJobRole != null && fs.DimJobRole.DimStandardJobRole != null &&
                                   fs.DimJobRole.DimStandardJobRole.IndustryFieldId == filterDto.IndustryFieldId.Value);
        }

        var dbModels = await query
            .AsNoTracking()
            .ToListAsync();

        return dbModels.Select(dbModel => FactSalaryConverter.ToDomain(dbModel));
    }

    public async Task UpdateFactSalaryAsync(FactSalary salaryFact)
    {
        var dbModel = await _dbContext.FactSalaries.FindAsync(salaryFact.SalaryFactId);
        if (dbModel is null)
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
            when (dbEx.InnerException is PostgresException pg &&
                  pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException("One or more referenced entities (date, city, employer, job role or employee) were not found during update.");
        }
    }

    public async Task DeleteFactSalaryByIdAsync(long salaryFactId)
    {
        var dbModel = await _dbContext.FactSalaries.FindAsync(salaryFactId);
        if (dbModel is null)
            throw new NotFoundException($"Salary fact with ID {salaryFactId} not found for deletion.");

        _dbContext.FactSalaries.Remove(dbModel);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<string?> GetBenchmarkingReportJsonAsync(BenchmarkQueryDto filters)
    {
        string granularityString = filters.Granularity.ToString().ToLowerInvariant();

        // BenchmarkQueryDto.DateStart/End are now DateOnly?
        // NpgsqlParameter with NpgsqlDbType.Date handles DateOnly directly.
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("p_industry_field_name_filter", NpgsqlDbType.Text) { Value = (object?)filters.IndustryFieldNameFilter ?? DBNull.Value },
            new NpgsqlParameter("p_standard_job_role_title_filter", NpgsqlDbType.Text) { Value = (object?)filters.StandardJobRoleTitleFilter ?? DBNull.Value },
            new NpgsqlParameter("p_hierarchy_level_name_filter", NpgsqlDbType.Text) { Value = (object?)filters.HierarchyLevelNameFilter ?? DBNull.Value },
            new NpgsqlParameter("p_district_name_filter", NpgsqlDbType.Text) { Value = (object?)filters.DistrictNameFilter ?? DBNull.Value },
            new NpgsqlParameter("p_oblast_name_filter", NpgsqlDbType.Text) { Value = (object?)filters.OblastNameFilter ?? DBNull.Value },
            new NpgsqlParameter("p_city_name_filter", NpgsqlDbType.Text) { Value = (object?)filters.CityNameFilter ?? DBNull.Value },
            new NpgsqlParameter("p_date_start", NpgsqlDbType.Date) { Value = (object?)filters.DateStart ?? DBNull.Value }, // Corrected: Direct use of DateOnly?
            new NpgsqlParameter("p_date_end", NpgsqlDbType.Date) { Value = (object?)filters.DateEnd ?? DBNull.Value },       // Corrected: Direct use of DateOnly?
            new NpgsqlParameter("p_target_percentile", NpgsqlDbType.Integer) { Value = filters.TargetPercentile },
            new NpgsqlParameter("p_granularity", NpgsqlDbType.Text) { Value = granularityString },
            new NpgsqlParameter("p_periods", NpgsqlDbType.Integer) { Value = filters.Periods }
        };

        var sql = @"SELECT marketstat.fn_get_benchmarking_data(
                        p_industry_field_name_filter   := @p_industry_field_name_filter,
                        p_standard_job_role_title_filter := @p_standard_job_role_title_filter,
                        p_hierarchy_level_name_filter  := @p_hierarchy_level_name_filter,
                        p_district_name_filter         := @p_district_name_filter,
                        p_oblast_name_filter           := @p_oblast_name_filter,
                        p_city_name_filter             := @p_city_name_filter,
                        p_date_start                   := @p_date_start,
                        p_date_end                     := @p_date_end,
                        p_target_percentile            := @p_target_percentile,
                        p_granularity                  := @p_granularity,
                        p_periods                      := @p_periods
                    );";

        string? jsonResult = null;
        var connection = _dbContext.Database.GetDbConnection();
        var closeConnection = false;
        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
                closeConnection = true;
            }
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
        finally
        {
            if (closeConnection && connection.State == ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }
        return jsonResult;
    }

    public async Task<List<SalaryDistributionBucketDto>> GetSalaryDistributionAsync(SalaryFilterDto filters)
    {
        return await _dbContext.Set<SalaryDistributionBucketDto>()
            .FromSqlInterpolated($"SELECT * FROM marketstat.fn_salary_distribution({filters.IndustryFieldId}, {filters.StandardJobRoleId}, {filters.HierarchyLevelId}, {filters.DistrictId}, {filters.OblastId}, {filters.CityId}, {filters.DateStart}, {filters.DateEnd})")
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SalarySummaryDto?> GetSalarySummaryAsync(SalaryFilterDto filters, int targetPercentile)
    {
        return await _dbContext.Set<SalarySummaryDto>()
            .FromSqlInterpolated($"SELECT * FROM marketstat.fn_salary_summary({filters.IndustryFieldId}, {filters.StandardJobRoleId}, {filters.HierarchyLevelId}, {filters.DistrictId}, {filters.OblastId}, {filters.CityId}, {filters.DateStart}, {filters.DateEnd}, {targetPercentile})")
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(SalaryFilterDto filters, TimeGranularity granularity, int periods)
    {
        string granularityString = granularity.ToString().ToLowerInvariant();
        return await _dbContext.Set<SalaryTimeSeriesPointDto>()
            .FromSqlInterpolated($"SELECT * FROM marketstat.fn_salary_time_series({filters.IndustryFieldId}, {filters.StandardJobRoleId}, {filters.HierarchyLevelId}, {filters.DistrictId}, {filters.OblastId}, {filters.CityId}, {filters.DateStart}, {filters.DateEnd}, {granularityString}, {periods})")
            .AsNoTracking()
            .ToListAsync();
    }
}