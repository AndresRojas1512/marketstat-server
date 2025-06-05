using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.MongoModels.MarketStat.Database.MongoModels.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MarketStat.Database.Repositories.MongoRepositories.Dimensions;

public class MongoDimCityRepository : IDimCityRepository
{
    private readonly IMongoCollection<DimCityMongoDocument> _citiesCollection;
    private readonly IMongoCollection<CounterDocument> _countersCollection;
    private readonly ILogger<MongoDimCityRepository> _logger;
    
    public MongoDimCityRepository(IMongoDatabase database, ILogger<MongoDimCityRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (database == null) throw new ArgumentNullException(nameof(database));
            
        _citiesCollection = database.GetCollection<DimCityMongoDocument>("dim_cities");
        _countersCollection = database.GetCollection<CounterDocument>("counters");
    }
    
    public async Task CreateIndexesAsync()
    {
        var cityIdIndexKey = Builders<DimCityMongoDocument>.IndexKeys.Ascending(x => x.CityId);
        await _citiesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimCityMongoDocument>(cityIdIndexKey, new CreateIndexOptions { Unique = true, Name = "idx_city_id_unique" })
        );

        var naturalKey = Builders<DimCityMongoDocument>.IndexKeys.Ascending(x => x.CityName).Ascending(x => x.OblastId);
        await _citiesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimCityMongoDocument>(naturalKey, new CreateIndexOptions { Unique = true, Name = "idx_city_name_oblast_id_unique" })
        );

        var oblastIdIndexKey = Builders<DimCityMongoDocument>.IndexKeys.Ascending(x => x.OblastId);
        await _citiesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DimCityMongoDocument>(oblastIdIndexKey, new CreateIndexOptions { Name = "idx_oblast_id" })
        );
        _logger.LogInformation("Ensured indexes for 'dim_cities' collection.");
    }
    
    private DimCity ToDomain(DimCityMongoDocument doc)
    {
        if (doc == null) return null!;
        return new DimCity(doc.CityId, doc.CityName, doc.OblastId);
    }
    
    private DimCityMongoDocument FromDomain(DimCity domain)
    {
        if (domain == null) return null!;
        return new DimCityMongoDocument
        {
            CityId = domain.CityId,
            CityName = domain.CityName,
            OblastId = domain.OblastId
        };
    }
    
    public async Task AddCityAsync(DimCity city)
    {
        _logger.LogInformation("MongoRepo: Attempting to add city: {CityName}, OblastId: {OblastId}", city.CityName, city.OblastId);
        if (city.CityId == 0)
        {
            city.CityId = await MongoSequenceHelper.GetNextSequenceValueAsync(_countersCollection, "city_id");
            _logger.LogInformation("MongoRepo: Generated new CityId {CityId} for {CityName}", city.CityId, city.CityName);
        }

        var document = FromDomain(city);
            
        try
        {
            await _citiesCollection.InsertOneAsync(document);
            _logger.LogInformation("MongoRepo: City '{CityName}' added with CityId {CityId}, ObjectId {ObjectId}", 
                document.CityName, document.CityId, document.Id);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error adding city '{CityName}'. It might already exist (name+oblast or CityId).", city.CityName);
            throw new ConflictException($"A city named '{city.CityName}' in oblast {city.OblastId} or with CityId {city.CityId} already exists.");
        }
    }
    
    public async Task<DimCity> GetCityByIdAsync(int cityId)
    {
        _logger.LogDebug("MongoRepo: Getting city by CityId: {CityId}", cityId);
        var filter = Builders<DimCityMongoDocument>.Filter.Eq(doc => doc.CityId, cityId);
        var document = await _citiesCollection.Find(filter).FirstOrDefaultAsync();
            
        if (document == null)
        {
            _logger.LogWarning("MongoRepo: City with CityId {CityId} not found.", cityId);
            throw new NotFoundException($"City with ID {cityId} not found");
        }
        return ToDomain(document);
    }
    
    public async Task<IEnumerable<DimCity>> GetAllCitiesAsync()
    {
        _logger.LogDebug("MongoRepo: Getting all cities.");
        var documents = await _citiesCollection.Find(FilterDefinition<DimCityMongoDocument>.Empty)
            .Sort(Builders<DimCityMongoDocument>.Sort.Ascending(x => x.CityName))
            .ToListAsync();
        return documents.Select(ToDomain);
    }
    
    public async Task<IEnumerable<DimCity>> GetCitiesByOblastIdAsync(int oblastId)
    {
        _logger.LogDebug("MongoRepo: Getting cities by OblastId: {OblastId}", oblastId);
        var filter = Builders<DimCityMongoDocument>.Filter.Eq(doc => doc.OblastId, oblastId);
        var documents = await _citiesCollection.Find(filter)
            .Sort(Builders<DimCityMongoDocument>.Sort.Ascending(x => x.CityName))
            .ToListAsync();
        return documents.Select(ToDomain);
    }
    
    public async Task UpdateCityAsync(DimCity city)
    {
        _logger.LogInformation("MongoRepo: Attempting to update city with CityId: {CityId}", city.CityId);
        var filter = Builders<DimCityMongoDocument>.Filter.Eq(doc => doc.CityId, city.CityId);

        var updateDocument = FromDomain(city);
        

        var existingDocument = await _citiesCollection.Find(filter).FirstOrDefaultAsync();
        if (existingDocument == null)
        {
            _logger.LogWarning("MongoRepo: City with CityId {CityId} not found for update.", city.CityId);
            throw new NotFoundException($"City with ID {city.CityId} not found for update.");
        }

        existingDocument.CityName = city.CityName;
        existingDocument.OblastId = city.OblastId;

        try
        {
            var result = await _citiesCollection.ReplaceOneAsync(filter, existingDocument);
            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("MongoRepo: City with CityId {CityId} not found during ReplaceOne.", city.CityId);
                throw new NotFoundException($"City with ID {city.CityId} not found for update (concurrent modification?).");
            }
            _logger.LogInformation("MongoRepo: City with CityId {CityId} updated. Matched: {Matched}, Modified: {Modified}", 
                                   city.CityId, result.MatchedCount, result.ModifiedCount);
        }
        catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(mwx, "MongoRepo: Duplicate key error updating city CityId {CityId} to name '{CityName}'.", city.CityId, city.CityName);
            throw new ConflictException($"A city named '{city.CityName}' in oblast {city.OblastId} likely already exists.");
        }
    }
    
    public async Task DeleteCityAsync(int cityId)
    {
        _logger.LogInformation("MongoRepo: Attempting to delete city with CityId: {CityId}", cityId);
        var filter = Builders<DimCityMongoDocument>.Filter.Eq(doc => doc.CityId, cityId);
        var result = await _citiesCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("MongoRepo: City with CityId {CityId} not found for deletion.", cityId);
            throw new NotFoundException($"City with ID {cityId} not found for deletion.");
        }
        _logger.LogInformation("MongoRepo: City with CityId {CityId} deleted. Count: {DeletedCount}", cityId, result.DeletedCount);
    }
}