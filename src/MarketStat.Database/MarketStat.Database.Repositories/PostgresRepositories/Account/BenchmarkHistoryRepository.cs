using System.Data;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Account;
using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace MarketStat.Database.Repositories.PostgresRepositories.Account;

public class BenchmarkHistoryRepository : IBenchmarkHistoryRepository
{
    private readonly MarketStatDbContext _dbContext;
    private readonly ILogger<BenchmarkHistoryRepository> _logger;

    public BenchmarkHistoryRepository(MarketStatDbContext dbContext, ILogger<BenchmarkHistoryRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<long> SaveBenchmarkAsync(int userId, SaveBenchmarkRequestDto saveRequest)
    {
        long newBenchmarkHistoryId = 0;

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("p_new_benchmark_history_id", NpgsqlDbType.Bigint) { Direction = ParameterDirection.Output },
            new NpgsqlParameter("p_user_id", NpgsqlDbType.Integer) { Value = userId },
            new NpgsqlParameter("p_benchmark_result_json", NpgsqlDbType.Jsonb) { Value = saveRequest.BenchmarkResultJson },
            new NpgsqlParameter("p_benchmark_name", NpgsqlDbType.Varchar) { Value = (object?)saveRequest.BenchmarkName ?? DBNull.Value },
            new NpgsqlParameter("p_filter_industry_field_id", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterIndustryFieldId ?? DBNull.Value },
            new NpgsqlParameter("p_filter_standard_job_role_id", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterStandardJobRoleId ?? DBNull.Value },
            new NpgsqlParameter("p_filter_hierarchy_level_id", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterHierarchyLevelId ?? DBNull.Value },
            new NpgsqlParameter("p_filter_district_id", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterDistrictId ?? DBNull.Value },
            new NpgsqlParameter("p_filter_oblast_id", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterOblastId ?? DBNull.Value },
            new NpgsqlParameter("p_filter_city_id", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterCityId ?? DBNull.Value },
            new NpgsqlParameter("p_filter_date_start", NpgsqlDbType.Date) { Value = (object?)saveRequest.FilterDateStart ?? DBNull.Value },
            new NpgsqlParameter("p_filter_date_end", NpgsqlDbType.Date) { Value = (object?)saveRequest.FilterDateEnd ?? DBNull.Value },
            new NpgsqlParameter("p_filter_target_percentile", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterTargetPercentile ?? DBNull.Value },
            new NpgsqlParameter("p_filter_granularity", NpgsqlDbType.Text) { Value = (object?)saveRequest.FilterGranularity ?? DBNull.Value },
            new NpgsqlParameter("p_filter_periods", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterPeriods ?? DBNull.Value }
        };
        
        _logger.LogDebug("Calling stored procedure marketstat.sp_save_benchmark with CommandType.StoredProcedure");
        foreach (var p in parameters)
        {
            var paramValueForLog = (p.Value == DBNull.Value && p.Direction != ParameterDirection.Output) ? "<DBNull>" : p.Value;
             _logger.LogDebug("Parameter: {ParameterName} = {ParameterValue} (Direction: {Direction})", p.ParameterName, paramValueForLog, p.Direction);
        }

        var connection = _dbContext.Database.GetDbConnection();
        var closeConnection = false;
        try
        {
            if (connection.State != ConnectionState.Open) { await connection.OpenAsync(); closeConnection = true; }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "marketstat.sp_save_benchmark"; // Just the procedure name
                command.CommandType = CommandType.StoredProcedure;  // Specify CommandType
                command.Parameters.AddRange(parameters.ToArray());
                
                await command.ExecuteNonQueryAsync();

                var outParam = command.Parameters["p_new_benchmark_history_id"] as NpgsqlParameter; 
                if (outParam?.Value != null && outParam.Value != DBNull.Value)
                {
                    newBenchmarkHistoryId = Convert.ToInt64(outParam.Value);
                }
                else
                {
                    _logger.LogError("Failed to retrieve new benchmark history ID. OUT parameter was null or DBNull.");
                    throw new ApplicationException("Failed to retrieve new benchmark history ID from stored procedure.");
                }
            }
        }
        catch (PostgresException pgEx)
        {
            _logger.LogError(pgEx, "PostgreSQL error saving benchmark. SQLState: {SqlState}, Details: {Detail}", pgEx.SqlState, pgEx.Detail);
            throw new ApplicationException($"Database error saving benchmark: {pgEx.MessageText}", pgEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while saving the benchmark.");
            throw new ApplicationException("An unexpected error occurred while saving the benchmark.", ex);
        }
        finally
        {
            if (closeConnection && connection.State == ConnectionState.Open) { await connection.CloseAsync(); }
        }
        
        if (newBenchmarkHistoryId <= 0) {
             _logger.LogError("Stored procedure did not return a valid positive new benchmark history ID. Returned: {NewId}", newBenchmarkHistoryId);
            throw new ApplicationException("Stored procedure did not return a valid new benchmark history ID.");
        }
        return newBenchmarkHistoryId;
    }

    public async Task<IEnumerable<BenchmarkHistory>> GetBenchmarksByUserIdAsync(int userId)
    {
        _logger.LogInformation("Repository: GetBenchmarksByUserIdAsync for UserId {UserId}", userId);
        var dbHistories = await _dbContext.BenchmarkHistories
            .Where(bh => bh.UserId == userId)
            .OrderByDescending(bh => bh.SavedAt)
            .AsNoTracking()
            .ToListAsync();

        _logger.LogInformation("Repository: Fetched {Count} dbHistories. Converting to domain.", dbHistories.Count);
        var domainList = dbHistories.Select(BenchmarkHistoryConverter.ToDomain).ToList();
        _logger.LogInformation("Repository: Converted to {Count} domain objects.", domainList.Count);
        return domainList;
    }

    public async Task<BenchmarkHistory> GetBenchmarkHistoryByIdAndUserIdAsync(long benchmarkHistoryId, int userId) 
    {
        _logger.LogInformation("Repository: GetBenchmarkHistoryByIdAndUserIdAsync for HistoryId {BenchmarkHistoryId}, UserId {UserId}", benchmarkHistoryId, userId);
        var dbHistory = await _dbContext.BenchmarkHistories
            .AsNoTracking()
            .FirstOrDefaultAsync(bh => bh.BenchmarkHistoryId == benchmarkHistoryId && bh.UserId == userId);
        if (dbHistory == null)
        {
            throw new NotFoundException($"Benchmark history with ID {benchmarkHistoryId} not found for user ID {userId}.");
        }
        return BenchmarkHistoryConverter.ToDomain(dbHistory);
    }

    public async Task DeleteBenchmarkHistoryAsync(long benchmarkHistoryId, int userId) 
    {
        var dbHistory = await _dbContext.BenchmarkHistories
            .FirstOrDefaultAsync(bh => bh.BenchmarkHistoryId == benchmarkHistoryId && bh.UserId == userId);

        if (dbHistory == null)
        {
            throw new NotFoundException($"Benchmark history with ID {benchmarkHistoryId} not found for user ID {userId} to delete.");
        }
        _dbContext.BenchmarkHistories.Remove(dbHistory);
        await _dbContext.SaveChangesAsync();
    }
}
