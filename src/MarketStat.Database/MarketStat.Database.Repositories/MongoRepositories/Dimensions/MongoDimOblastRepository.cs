using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimOblastRepository : IDimOblastRepository
{
    private readonly IMongoCollection<DimOblastMongoDocument> _oblastsCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoDimOblastRepository> _logger;

    public MongoDimOblastRepository(IMongoDatabase database, ILogger<MongoDimOblastRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _oblastsCollection = database.GetCollection<DimOblastMongoDocument>("dim_oblasts");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var oblastIdIndex = Builders<DimOblastMongoDocument>.IndexKeys.Ascending(x => x.OblastId);
        await _oblastsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimOblastMongoDocument>(oblastIdIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_oblast_id_unique" })
        );

        var nameIndex = Builders<DimOblastMongoDocument>.IndexKeys.Ascending(x => x.OblastName);
        await _oblastsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimOblastMongoDocument>(nameIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_oblast_name_unique" })
        );

        var districtIdIndex = Builders<DimOblastMongoDocument>.IndexKeys.Ascending(x => x.DistrictId);
        await _oblastsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimOblastMongoDocument>(districtIdIndex, 
            new CreateIndexOptions { Name = "idx_district_id_for_oblasts" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_oblasts' collection.");
    }

    private DimOblast ToDomain(DimOblastMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimOblast
        {
            OblastId = doc.OblastId,
            OblastName = doc.OblastName,
            DistrictId = doc.DistrictId
        };
    }

    private DimOblastMongoDocument FromDomain(DimOblast domain)
    {
        if (domain == null) return null!;
        return new DimOblastMongoDocument
        {
            OblastId = domain.OblastId,
            OblastName = domain.OblastName,
            DistrictId = domain.DistrictId
        };
    }
    
    public async Task AddOblastAsync(DimOblast dimOblast)
    {
        _logger.LogInformation("MongoRepo: Attempting to add oblast: {OblastName}", dimOblast.OblastName);
        if (dimOblast.OblastId == 0)
        {
            dimOblast.OblastId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "oblast_id");
            _logger.LogInformation("MongoRepo: Generated new OblastId {OblastId} for {OblastName}", 
                                   dimOblast.OblastId, dimOblast.OblastName);
        }

        var document = FromDomain(dimOblast);
        
        try
        {
            await _oblastsCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Oblast '{OblastName}' added with OblastId {OblastId}, ObjectId {ObjectId}", 
                                   document.OblastName, document.OblastId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding oblast '{OblastName}'. It might already exist (name or OblastId).", dimOblast.OblastName);
            string errorMessage = $"An oblast with the name '{dimOblast.OblastName}' or ID {dimOblast.OblastId} already exists.";
             if(mwx.Message.Contains("idx_oblast_name_unique")) errorMessage = $"An oblast named '{dimOblast.OblastName}' already exists.";
             else if (mwx.Message.Contains("idx_oblast_id_unique")) errorMessage = $"OblastId {dimOblast.OblastId} conflict.";
            throw new ConflictException(errorMessage);
        }
    }

    public async Task<DimOblast> GetOblastByIdAsync(int id)
    {
        _logger.LogDebug("MongoRepo: Getting oblast by OblastId: {Id}", id);
        var filter = Builders<DimOblastMongoDocument>.Filter.Eq(doc => doc.OblastId, id);
        var document = await _oblastsCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Oblast with OblastId {Id} not found.", id);
            throw new NotFoundException($"Oblast with ID {id} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimOblast>> GetAllOblastsAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all oblasts.");
        var documents = await _oblastsCollection.Find(FilterDefinition<DimOblastMongoDocument>.Empty)
                                                 .Sort(Builders<DimOblastMongoDocument>.Sort.Ascending(x => x.OblastName))
                                                 .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task<IEnumerable<DimOblast>> GetOblastsByFederalDistrictIdAsync(int districtId)
    {
        _logger.LogDebug("MongoRepo: Getting oblasts by FederalDistrictId: {DistrictId}", districtId);
        var filter = Builders<DimOblastMongoDocument>.Filter.Eq(doc => doc.DistrictId, districtId);
        var documents = await _oblastsCollection.Find(filter)
                                             .Sort(Builders<DimOblastMongoDocument>.Sort.Ascending(x => x.OblastName))
                                             .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateOblastAsync(DimOblast dimOblast)
    {
        _logger.LogInformation("MongoRepo: Attempting to update oblast with OblastId: {Id}", dimOblast.OblastId);
        var filter = Builders<DimOblastMongoDocument>.Filter.Eq(doc => doc.OblastId, dimOblast.OblastId);
        
        var existingDocument = await _oblastsCollection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
            _logger.LogWarning("MongoRepo: OblastId {Id} not found for update.", dimOblast.OblastId);
            throw new NotFoundException($"Oblast with ID {dimOblast.OblastId} not found for update.");
        }
        
        existingDocument.OblastName = dimOblast.OblastName;
        existingDocument.DistrictId = dimOblast.DistrictId;

        try
        {
            var result = await _oblastsCollection.ReplaceOneAsync(filter, existingDocument);
            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("MongoRepo: OblastId {Id} not matched during ReplaceOne operation.", dimOblast.OblastId);
                throw new NotFoundException($"Oblast with ID {dimOblast.OblastId} not found for update (concurrent modification?).");
            }
            _logger.LogInformation("MongoRepo: Oblast with OblastId {Id} updated. Matched: {Matched}, Modified: {Modified}", 
                                   dimOblast.OblastId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating oblast OblastId {Id} to name '{Name}'.", 
                               dimOblast.OblastId, dimOblast.OblastName);
            throw new ConflictException($"An oblast named '{dimOblast.OblastName}' already exists.");
        }
    }

    public async Task DeleteOblastAsync(int id)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete oblast with OblastId: {Id}", id);
        var filter = Builders<DimOblastMongoDocument>.Filter.Eq(doc => doc.OblastId, id);
        var result = await _oblastsCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("MongoRepo: Oblast with OblastId {Id} not found for deletion.", id);
            throw new NotFoundException($"Oblast with ID {id} not found for deletion.");
        }
        _logger.LogInformation("MongoRepo: Oblast with OblastId {Id} deleted. Count: {DeletedCount}", 
                               id, result.DeletedCount);
    }
}