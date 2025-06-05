using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimFederalDistrictRepository : IDimFederalDistrictRepository
{
    private readonly IMongoCollection<DimFederalDistrictMongoDocument> _districtsCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoDimFederalDistrictRepository> _logger;

    public MongoDimFederalDistrictRepository(IMongoDatabase database, ILogger<MongoDimFederalDistrictRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
        
        _districtsCollection = database.GetCollection<DimFederalDistrictMongoDocument>("dim_federal_districts");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }

    public async Task CreateIndexesAsync()
    {
        var districtIdIndex = Builders<DimFederalDistrictMongoDocument>.IndexKeys.Ascending(x => x.DistrictId);
        await _districtsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimFederalDistrictMongoDocument>(districtIdIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_district_id_unique" })
        );

        var nameIndex = Builders<DimFederalDistrictMongoDocument>.IndexKeys.Ascending(x => x.DistrictName);
        await _districtsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimFederalDistrictMongoDocument>(nameIndex, 
            new CreateIndexOptions { Unique = true, Name = "idx_district_name_unique" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_federal_districts' collection.");
    }

    private DimFederalDistrict ToDomain(DimFederalDistrictMongoDocument? doc)
    {
        if (doc == null) return null!;
        return new DimFederalDistrict
        {
            DistrictId = doc.DistrictId,
            DistrictName = doc.DistrictName
        };
    }

    private DimFederalDistrictMongoDocument FromDomain(DimFederalDistrict domain)
    {
        if (domain == null) return null!;
        return new DimFederalDistrictMongoDocument
        {
            DistrictId = domain.DistrictId,
            DistrictName = domain.DistrictName
        };
    }
    
    public async Task AddFederalDistrictAsync(DimFederalDistrict district)
    {
        _logger.LogInformation("MongoRepo: Attempting to add federal district: {DistrictName}", district.DistrictName);
        if (district.DistrictId == 0)
        {
            district.DistrictId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "federal_district_id");
            _logger.LogInformation("MongoRepo: Generated new DistrictId {DistrictId} for {DistrictName}", 
                                   district.DistrictId, district.DistrictName);
        }

        var document = FromDomain(district);
        
        try
        {
            await _districtsCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: Federal district '{DistrictName}' added with DistrictId {DistrictId}, ObjectId {ObjectId}", 
                                   document.DistrictName, document.DistrictId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding federal district '{DistrictName}'. It might already exist (name or DistrictId).", district.DistrictName);
            string errorMessage = $"A federal district with the name '{district.DistrictName}' or ID {district.DistrictId} already exists.";
             if(mwx.Message.Contains("idx_district_name_unique")) errorMessage = $"A federal district named '{district.DistrictName}' already exists.";
             else if (mwx.Message.Contains("idx_district_id_unique")) errorMessage = $"DistrictId {district.DistrictId} conflict.";
            throw new ConflictException(errorMessage);
        }
    }

    public async Task<DimFederalDistrict> GetFederalDistrictByIdAsync(int id)
    {
        _logger.LogDebug("MongoRepo: Getting federal district by DistrictId: {Id}", id);
        var filter = Builders<DimFederalDistrictMongoDocument>.Filter.Eq(doc => doc.DistrictId, id);
        var document = await _districtsCollection.Find(filter).FirstOrDefaultAsync();
        
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: Federal district with DistrictId {Id} not found.", id);
            throw new NotFoundException($"Federal district with ID {id} not found.");
        }
        return ToDomain(document);
    }

    public async Task<IEnumerable<DimFederalDistrict>> GetAllFederalDistrictsAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all federal districts.");
        var documents = await _districtsCollection.Find(FilterDefinition<DimFederalDistrictMongoDocument>.Empty)
                                                 .Sort(Builders<DimFederalDistrictMongoDocument>.Sort.Ascending(x => x.DistrictName))
                                                 .ToListAsync();
        return documents.Select(ToDomain);
    }

    public async Task UpdateFederalDistrictAsync(DimFederalDistrict district)
    {
        _logger.LogInformation("MongoRepo: Attempting to update federal district with DistrictId: {Id}", district.DistrictId);
        var filter = Builders<DimFederalDistrictMongoDocument>.Filter.Eq(doc => doc.DistrictId, district.DistrictId);
        
        var existingDocument = await _districtsCollection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
             _logger.LogWarning("MongoRepo: DistrictId {Id} not found for update.", district.DistrictId);
            throw new NotFoundException($"Federal district with ID {district.DistrictId} not found for update.");
        }
        
        existingDocument.DistrictName = district.DistrictName;

        try
        {
            var result = await _districtsCollection.ReplaceOneAsync(filter, existingDocument);
            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("MongoRepo: DistrictId {Id} not matched during ReplaceOne operation.", district.DistrictId);
                throw new NotFoundException($"Federal district with ID {district.DistrictId} not found for update (concurrent modification?).");
            }
             _logger.LogInformation("MongoRepo: Federal district with DistrictId {Id} updated. Matched: {Matched}, Modified: {Modified}", 
                                   district.DistrictId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating federal district DistrictId {Id} to name '{Name}'.", 
                               district.DistrictId, district.DistrictName);
            throw new ConflictException($"A federal district named '{district.DistrictName}' already exists.");
        }
    }

    public async Task DeleteFederalDistrictAsync(int id)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete federal district with DistrictId: {Id}", id);
        var filter = Builders<DimFederalDistrictMongoDocument>.Filter.Eq(doc => doc.DistrictId, id);
        var result = await _districtsCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("MongoRepo: Federal district with DistrictId {Id} not found for deletion.", id);
            throw new NotFoundException($"Federal district with ID {id} not found for deletion.");
        }
        _logger.LogInformation("MongoRepo: Federal district with DistrictId {Id} deleted. Count: {DeletedCount}", 
                               id, result.DeletedCount);
    }
}