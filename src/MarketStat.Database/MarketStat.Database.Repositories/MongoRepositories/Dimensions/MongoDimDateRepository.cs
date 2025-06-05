using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimDateRepository : IDimDateRepository
{
    private readonly IMongoCollection<DimDateMongoDocument> _datesCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoDimDateRepository> _logger;

    public MongoDimDateRepository(IMongoDatabase database, ILogger<MongoDimDateRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _datesCollection = database.GetCollection<DimDateMongoDocument>("dim_dates");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var dateIdIndex = Builders<DimDateMongoDocument>.IndexKeys.Ascending(x => x.DateId);
        await _datesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimDateMongoDocument>(dateIdIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_date_id_unique" })
        );

        var fullDateIndex = Builders<DimDateMongoDocument>.IndexKeys.Ascending(x => x.FullDate);
        await _datesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimDateMongoDocument>(fullDateIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_full_date_unique" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_dates' collection.");
    }

    private DimDate ToDomain(DimDateMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimDate
        {
            DateId = doc.DateId,
            FullDate = DateOnly.FromDateTime(doc.FullDate),
            Year = (short)doc.Year,
            Quarter = (short)doc.Quarter,
            Month = (short)doc.Month
        };
    }

    private DimDateMongoDocument FromDomain(DimDate domainDate)
    {
        if (domainDate == null) return null!;
        return new DimDateMongoDocument
        {
            DateId = domainDate.DateId,
            FullDate = domainDate.FullDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), 
            Year = domainDate.Year,
            Quarter = domainDate.Quarter,
            Month = domainDate.Month
        };
    }

    public async Task AddDateAsync(DimDate date) // Parameter name in interface is 'date'
    {
        _logger.LogInformation("MongoRepo: Attempting to add date: {FullDate}", date.FullDate);
        if (date.DateId == 0)
        {
            date.DateId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "date_id");
            _logger.LogInformation("MongoRepo: Generated new DateId {DateId} for {FullDate}", date.DateId, date.FullDate);
        }

        var document = FromDomain(date);
        
        try
        {
            await _datesCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Date '{FullDate}' added with DateId {DateId}, ObjectId {ObjectId}", 
                                   document.FullDate, document.DateId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding date '{FullDate}'. It might already exist (FullDate or DateId).", date.FullDate);
            string errorMessage = $"A date record for {date.FullDate:yyyy-MM-dd} or with DateId {date.DateId} already exists.";
             if (mwx.Message.Contains("idx_full_date_unique")) errorMessage = $"A date record for {date.FullDate:yyyy-MM-dd} already exists.";
             else if (mwx.Message.Contains("idx_date_id_unique")) errorMessage = $"DateId {date.DateId} conflict (should not happen with sequence).";
            throw new ConflictException(errorMessage);
        }
    }

    public async Task<DimDate> GetDateByIdAsync(int dateId)
    {
        _logger.LogDebug("MongoRepo: Getting date by DateId: {DateId}", dateId);
        var filter = Builders<DimDateMongoDocument>.Filter.Eq(doc => doc.DateId, dateId);
        var document = await _datesCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Date with DateId {DateId} not found.", dateId);
            throw new NotFoundException($"Date with ID {dateId} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimDate>> GetAllDatesAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all dates.");
        var documents = await _datesCollection.Find(FilterDefinition<DimDateMongoDocument>.Empty)
                                             .Sort(Builders<DimDateMongoDocument>.Sort.Ascending(x => x.FullDate))
                                             .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateDateAsync(DimDate date)
    {
        _logger.LogInformation("MongoRepo: Attempting to update date with DateId: {DateId}", date.DateId);
        var filter = Builders<DimDateMongoDocument>.Filter.Eq(doc => doc.DateId, date.DateId);
        
        var documentToUpdate = FromDomain(date);
        
        var updateDefinition = Builders<DimDateMongoDocument>.Update
            .Set(doc => doc.Year, date.Year)
            .Set(doc => doc.Quarter, date.Quarter)
            .Set(doc => doc.Month, date.Month)
            .Set(doc => doc.FullDate, date.FullDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

        try
        {
            var result = await _datesCollection.UpdateOneAsync(filter, updateDefinition);

            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("MongoRepo: Date with DateId {DateId} not found for update.", date.DateId);
                throw new NotFoundException($"Date with ID {date.DateId} not found for update.");
            }
             _logger.LogInformation("MongoRepo: Date with DateId {DateId} updated. Matched: {Matched}, Modified: {Modified}", 
                                   date.DateId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating date DateId {DateId} (likely new FullDate conflicts).", date.DateId);
            throw new ConflictException($"Updating date record for {date.FullDate:yyyy-MM-dd} caused a conflict.");
        }
    }

    public async Task DeleteDateAsync(int dateId)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete date with DateId: {DateId}", dateId);
        var filter = Builders<DimDateMongoDocument>.Filter.Eq(doc => doc.DateId, dateId);
        var result = await _datesCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("MongoRepo: Date with DateId {DateId} not found for deletion.", dateId);
            throw new NotFoundException($"Date with ID {dateId} not found for deletion.");
        }
        _logger.LogInformation("MongoRepo: Date with DateId {DateId} deleted. Count: {DeletedCount}", dateId, result.DeletedCount);
    }
}