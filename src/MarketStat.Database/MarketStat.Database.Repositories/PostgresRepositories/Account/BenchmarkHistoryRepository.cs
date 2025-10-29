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

public class BenchmarkHistoryRepository : BaseRepository, IBenchmarkHistoryRepository
{
    private readonly MarketStatDbContext _dbContext;
    private readonly ILogger<BenchmarkHistoryRepository> _logger;

    public BenchmarkHistoryRepository(MarketStatDbContext dbContext, ILogger<BenchmarkHistoryRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BenchmarkHistory> SaveBenchmarkAsync(BenchmarkHistory benchmarkToSave)
    {
        _logger.LogInformation("Repository: Saving benchmark for User {UserId}, Benchmark Name: {BenchmarkName}",
            benchmarkToSave.UserId, benchmarkToSave.BenchmarkName);
        
        var dbModel = BenchmarkHistoryConverter.ToDbModel(benchmarkToSave);
        dbModel.SavedAt = DateTimeOffset.UtcNow;

        bool userExists = await _dbContext.Users.AnyAsync(u => u.UserId == benchmarkToSave.UserId);
        if (!userExists)
        {
            _logger.LogWarning("Attempted to save benchmark for non-existent User ID: {UserId}",
                benchmarkToSave.UserId);
            throw new NotFoundException($"User with ID {benchmarkToSave.UserId} not found.");
        }
        
        await _dbContext.BenchmarkHistories.AddAsync(dbModel);

        try
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Successfully saved benchmark. New BenchmarkHistoryId: {BenchmarkHistoryId}",
                dbModel.BenchmarkHistoryId);
            benchmarkToSave.BenchmarkHistoryId = dbModel.BenchmarkHistoryId;
            benchmarkToSave.SavedAt = dbModel.SavedAt;
            return benchmarkToSave;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Datebase error occurred while saving benchmark history for User {UserId}",
                benchmarkToSave.UserId);
            throw new ApplicationException("An error occured while saving the benchmark.", ex);
        }
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
        return dbHistories.Select(BenchmarkHistoryConverter.ToDomain).ToList();
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
        _logger.LogInformation("Successfully deleted benchmark history {BenchmarkHistoryId} for User {UserId}",
            benchmarkHistoryId, userId);
    }
}
