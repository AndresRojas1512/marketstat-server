using System.Data;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Account;
using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Account;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace MarketStat.Database.Repositories.PostgresRepositories.Account;

public class BenchmarkHistoryRepository : IBenchmarkHistoryRepository
{
    private readonly MarketStatDbContext _dbContext;

    public BenchmarkHistoryRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<long> SaveBenchmarkAsync(int userId, SaveBenchmarkRequestDto saveRequest)
    {
        long newBenchmarkHistoryId = 0;

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("p_new_benchmark_history_id_out", NpgsqlDbType.Bigint) { Direction = ParameterDirection.Output },
            new NpgsqlParameter("p_user_id_in", NpgsqlDbType.Integer) { Value = userId },
            new NpgsqlParameter("p_benchmark_result_json_in", NpgsqlDbType.Jsonb) { Value = saveRequest.BenchmarkResultJson },
            new NpgsqlParameter("p_benchmark_name_in", NpgsqlDbType.Varchar) { Value = (object?)saveRequest.BenchmarkName ?? DBNull.Value },
            
            // ID-based filter parameters
            new NpgsqlParameter("p_filter_industry_field_id_in", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterIndustryFieldId ?? DBNull.Value },
            new NpgsqlParameter("p_filter_standard_job_role_id_in", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterStandardJobRoleId ?? DBNull.Value },
            new NpgsqlParameter("p_filter_hierarchy_level_id_in", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterHierarchyLevelId ?? DBNull.Value },
            new NpgsqlParameter("p_filter_district_id_in", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterDistrictId ?? DBNull.Value },
            new NpgsqlParameter("p_filter_oblast_id_in", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterOblastId ?? DBNull.Value },
            new NpgsqlParameter("p_filter_city_id_in", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterCityId ?? DBNull.Value },
            
            new NpgsqlParameter("p_filter_date_start_in", NpgsqlDbType.Date) { Value = (object?)saveRequest.FilterDateStart ?? DBNull.Value },
            new NpgsqlParameter("p_filter_date_end_in", NpgsqlDbType.Date) { Value = (object?)saveRequest.FilterDateEnd ?? DBNull.Value },
            new NpgsqlParameter("p_filter_target_percentile_in", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterTargetPercentile ?? DBNull.Value },
            new NpgsqlParameter("p_filter_granularity_in", NpgsqlDbType.Text) { Value = (object?)saveRequest.FilterGranularity ?? DBNull.Value },
            new NpgsqlParameter("p_filter_periods_in", NpgsqlDbType.Integer) { Value = (object?)saveRequest.FilterPeriods ?? DBNull.Value }
        };
        
        var sql = @"CALL marketstat.sp_save_benchmark(
                        @p_new_benchmark_history_id_out, @p_user_id_in, @p_benchmark_result_json_in,
                        @p_benchmark_name_in, 
                        @p_filter_industry_field_id_in, @p_filter_standard_job_role_id_in, 
                        @p_filter_hierarchy_level_id_in, @p_filter_district_id_in,
                        @p_filter_oblast_id_in, @p_filter_city_id_in,
                        @p_filter_date_start_in, @p_filter_date_end_in,
                        @p_filter_target_percentile_in, @p_filter_granularity_in, @p_filter_periods_in
                    );";

        var connection = _dbContext.Database.GetDbConnection();
        var closeConnection = false;
        try
        {
            if (connection.State != ConnectionState.Open) { await connection.OpenAsync(); closeConnection = true; }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(parameters.ToArray());
                
                await command.ExecuteNonQueryAsync();

                var outParam = command.Parameters["p_new_benchmark_history_id_out"] as NpgsqlParameter;
                if (outParam?.Value != null && outParam.Value != DBNull.Value)
                {
                    newBenchmarkHistoryId = Convert.ToInt64(outParam.Value);
                }
                else
                {
                    throw new ApplicationException("Failed to retrieve new benchmark history ID from stored procedure. The OUT parameter was not set.");
                }
            }
        }
        catch (PostgresException pgEx)
        {
            throw new ApplicationException($"Database error saving benchmark: {pgEx.MessageText}", pgEx);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An unexpected error occurred while saving the benchmark.", ex);
        }
        finally
        {
            if (closeConnection && connection.State == ConnectionState.Open) { await connection.CloseAsync(); }
        }
        
        if (newBenchmarkHistoryId <= 0) {
            throw new ApplicationException("Stored procedure did not return a valid new benchmark history ID.");
        }
        return newBenchmarkHistoryId;
    }

    public async Task<IEnumerable<BenchmarkHistory>> GetBenchmarksByUserIdAsync(int userId)
    {
        var dbHistories = await _dbContext.BenchmarkHistories
            .Where(bh => bh.UserId == userId)
            .Include(bh => bh.User)
            .OrderByDescending(bh => bh.SavedAt)
            .AsNoTracking()
            .ToListAsync();
            
        return dbHistories.Select(BenchmarkHistoryConverter.ToDomain).ToList();
    }

    public async Task<BenchmarkHistory> GetBenchmarkHistoryByIdAndUserIdAsync(long benchmarkHistoryId, int userId)
    {
        var dbHistory = await _dbContext.BenchmarkHistories
            .Include(bh => bh.User)
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
