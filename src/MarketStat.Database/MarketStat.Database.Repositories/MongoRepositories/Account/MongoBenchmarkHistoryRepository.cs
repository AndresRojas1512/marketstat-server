using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Account;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Account;

public class MongoBenchmarkHistoryRepository : IBenchmarkHistoryRepository
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoCollection<BenchmarkHistoryMongoDocument> _benchmarksCollection;
    private readonly IMongoCollection<UserMongoDocument> _usersCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoBenchmarkHistoryRepository> _logger;

    public MongoBenchmarkHistoryRepository(IMongoClient mongoClient, IMongoDatabase database, ILogger<MongoBenchmarkHistoryRepository> logger)
    {
        _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _benchmarksCollection = database.GetCollection<BenchmarkHistoryMongoDocument>("benchmark_histories");
        _usersCollection = database.GetCollection<UserMongoDocument>("users");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var historyIdIndex = Builders<BenchmarkHistoryMongoDocument>.IndexKeys.Ascending(x => x.BenchmarkHistoryId);
        await _benchmarksCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<BenchmarkHistoryMongoDocument>(historyIdIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_benchmark_history_id_unique" })
        );

        var userIdIndex = Builders<BenchmarkHistoryMongoDocument>.IndexKeys.Ascending(x => x.UserId);
        await _benchmarksCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<BenchmarkHistoryMongoDocument>(userIdIndex, 
            new CreateIndexOptions { Name = "idx_benchmark_history_user_id" })
        );

        var savedAtIndex = Builders<BenchmarkHistoryMongoDocument>.IndexKeys.Descending(x => x.SavedAt);
         await _benchmarksCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<BenchmarkHistoryMongoDocument>(savedAtIndex, 
            new CreateIndexOptions { Name = "idx_benchmark_history_saved_at" })
        );
        _logger.LogInformation("Ensured indexes for 'benchmark_histories' collection.");
    }

    private BenchmarkHistory ToDomain(BenchmarkHistoryMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new BenchmarkHistory
        {
            BenchmarkHistoryId = doc.BenchmarkHistoryId,
            UserId = doc.UserId,
            BenchmarkName = doc.BenchmarkName,
            SavedAt = doc.SavedAt,
            FilterIndustryFieldId = doc.FilterIndustryFieldId,
            FilterStandardJobRoleId = doc.FilterStandardJobRoleId,
            FilterHierarchyLevelId = doc.FilterHierarchyLevelId,
            FilterDistrictId = doc.FilterDistrictId,
            FilterOblastId = doc.FilterOblastId,
            FilterCityId = doc.FilterCityId,
            FilterDateStart = !string.IsNullOrEmpty(doc.FilterDateStart) ? DateOnly.Parse(doc.FilterDateStart) : null,
            FilterDateEnd = !string.IsNullOrEmpty(doc.FilterDateEnd) ? DateOnly.Parse(doc.FilterDateEnd) : null,
            FilterTargetPercentile = doc.FilterTargetPercentile,
            FilterGranularity = doc.FilterGranularity, 
            FilterPeriods = doc.FilterPeriods,
            BenchmarkResultJson = doc.BenchmarkResultJson
        };
    }

    private BenchmarkHistoryMongoDocument FromSaveRequest(int userId, SaveBenchmarkRequestDto saveRequest, long newHistoryId)
    {
        return new BenchmarkHistoryMongoDocument
        {
            BenchmarkHistoryId = newHistoryId,
            UserId = userId,
            BenchmarkName = saveRequest.BenchmarkName,
            SavedAt = DateTimeOffset.UtcNow,
            FilterIndustryFieldId = saveRequest.FilterIndustryFieldId,
            FilterStandardJobRoleId = saveRequest.FilterStandardJobRoleId,
            FilterHierarchyLevelId = saveRequest.FilterHierarchyLevelId,
            FilterDistrictId = saveRequest.FilterDistrictId,
            FilterOblastId = saveRequest.FilterOblastId,
            FilterCityId = saveRequest.FilterCityId,
            FilterDateStart = saveRequest.FilterDateStart?.ToString("yyyy-MM-dd"),
            FilterDateEnd = saveRequest.FilterDateEnd?.ToString("yyyy-MM-dd"),
            FilterTargetPercentile = saveRequest.FilterTargetPercentile,
            FilterGranularity = saveRequest.FilterGranularity,
            FilterPeriods = saveRequest.FilterPeriods,
            BenchmarkResultJson = saveRequest.BenchmarkResultJson
        };
    }

    public async Task<long> SaveBenchmarkAsync(int userId, SaveBenchmarkRequestDto saveRequest)
    {
        _logger.LogInformation("MongoRepo: Attempting to save benchmark for UserId {UserId}, Name: {BenchmarkName}", userId, saveRequest.BenchmarkName);
        
        long newBenchmarkHistoryId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "benchmark_history_id");
        var document = FromSaveRequest(userId, saveRequest, newBenchmarkHistoryId);

        using (var session = await _mongoClient.StartSessionAsync())
        {
            session.StartTransaction(new TransactionOptions(
                readConcern: ReadConcern.Snapshot,
                writeConcern: WriteConcern.WMajority));
            try
            {
                await _benchmarksCollection.InsertOneAsync(session, document);
                _logger.LogInformation("MongoRepo: BenchmarkHistory document inserted with ID {NewId}", newBenchmarkHistoryId);

                var userFilter = Builders<UserMongoDocument>.Filter.Eq(u => u.UserId, userId);
                var userUpdate = Builders<UserMongoDocument>.Update.Inc(u => u.SavedBenchmarksCount, 1);
                var updateUserResult = await _usersCollection.UpdateOneAsync(session, userFilter, userUpdate);

                if (updateUserResult.MatchedCount == 0)
                {
                    _logger.LogError("MongoRepo: User with UserId {UserId} not found to increment benchmark count. Rolling back transaction.", userId);
                    await session.AbortTransactionAsync();
                    throw new NotFoundException($"User {userId} not found when trying to update benchmark count.");
                }
                
                await session.CommitTransactionAsync();
                _logger.LogInformation("MongoRepo: Benchmark saved with HistoryId {HistoryId} and user count incremented for UserId {UserId}. Transaction committed.", 
                                       newBenchmarkHistoryId, userId);
                return newBenchmarkHistoryId;
            }
            catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                await session.AbortTransactionAsync();
                _logger.LogWarning(mwx, "MongoRepo: Duplicate key error saving benchmark (likely BenchmarkHistoryId conflict). HistoryId attempt: {AttemptedId}", newBenchmarkHistoryId);
                throw new ConflictException($"A benchmark history record with ID {newBenchmarkHistoryId} already exists (should not happen with sequence).");
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "MongoRepo: Error saving benchmark for UserId {UserId}. Transaction aborted.", userId);
                throw;
            }
        }
    }

    public async Task<IEnumerable<BenchmarkHistory>> GetBenchmarksByUserIdAsync(int userId)
    {
        _logger.LogDebug("MongoRepo: Getting benchmarks for UserId: {UserId}", userId);
        var filter = Builders<BenchmarkHistoryMongoDocument>.Filter.Eq(doc => doc.UserId, userId);
        var documents = await _benchmarksCollection.Find(filter)
                                                 .Sort(Builders<BenchmarkHistoryMongoDocument>.Sort.Descending(x => x.SavedAt))
                                                 .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task<BenchmarkHistory> GetBenchmarkHistoryByIdAndUserIdAsync(long benchmarkHistoryId, int userId)
    {
        _logger.LogDebug("MongoRepo: Getting benchmark HistoryId {HistoryId} for UserId {UserId}", benchmarkHistoryId, userId);
        var filter = Builders<BenchmarkHistoryMongoDocument>.Filter.And(
            Builders<BenchmarkHistoryMongoDocument>.Filter.Eq(doc => doc.BenchmarkHistoryId, benchmarkHistoryId),
            Builders<BenchmarkHistoryMongoDocument>.Filter.Eq(doc => doc.UserId, userId)
        );
        var document = await _benchmarksCollection.Find(filter).FirstOrDefaultAsync();

        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Benchmark history with ID {HistoryId} for UserID {UserId} not found.", benchmarkHistoryId, userId);
            throw new NotFoundException($"Benchmark history with ID {benchmarkHistoryId} not found for user ID {userId}.");
        }
        return ToDomain(document);
    }

    public async Task DeleteBenchmarkHistoryAsync(long benchmarkHistoryId, int userId)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete benchmark HistoryId {HistoryId} for UserId {UserId}", benchmarkHistoryId, userId);
        
        using (var session = await _mongoClient.StartSessionAsync())
        {
            session.StartTransaction(new TransactionOptions(
                readConcern: ReadConcern.Snapshot,
                writeConcern: WriteConcern.WMajority));
            try
            {
                var filter = Builders<BenchmarkHistoryMongoDocument>.Filter.And(
                    Builders<BenchmarkHistoryMongoDocument>.Filter.Eq(doc => doc.BenchmarkHistoryId, benchmarkHistoryId),
                    Builders<BenchmarkHistoryMongoDocument>.Filter.Eq(doc => doc.UserId, userId)
                );
                var deleteResult = await _benchmarksCollection.DeleteOneAsync(session, filter);

                if (deleteResult.DeletedCount == 0)
                {
                    await session.AbortTransactionAsync(); // Abort if nothing was deleted
                    _logger.LogWarning("MongoRepo: Benchmark history with ID {HistoryId} for UserID {UserId} not found for deletion.", benchmarkHistoryId, userId);
                    throw new NotFoundException($"Benchmark history with ID {benchmarkHistoryId} not found for user ID {userId} to delete.");
                }

                // Atomically decrement user's benchmark count
                var userFilter = Builders<UserMongoDocument>.Filter.Eq(u => u.UserId, userId);
                var userUpdate = Builders<UserMongoDocument>.Update.Inc(u => u.SavedBenchmarksCount, -1);
                var updateUserResult = await _usersCollection.UpdateOneAsync(session, userFilter, userUpdate);

                if (updateUserResult.MatchedCount == 0)
                {
                    // This is problematic: benchmark deleted but user count not updated.
                    // The transaction should prevent this state if user doesn't exist.
                    _logger.LogError("MongoRepo: User with UserId {UserId} not found to decrement benchmark count, but benchmark was deleted. Transaction will be aborted.", userId);
                    await session.AbortTransactionAsync();
                    throw new ApplicationException($"User {userId} not found for benchmark count update, but benchmark was deleted. Data inconsistency risk.");
                }
                
                await session.CommitTransactionAsync();
                _logger.LogInformation("MongoRepo: Benchmark HistoryId {HistoryId} deleted and user count decremented for UserId {UserId}. Transaction committed.", 
                                       benchmarkHistoryId, userId);
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "MongoRepo: Error deleting benchmark HistoryId {HistoryId} for UserId {UserId}. Transaction aborted.", benchmarkHistoryId, userId);
                throw;
            }
        }
    }
}